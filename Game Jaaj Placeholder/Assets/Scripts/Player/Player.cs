using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Player : MonoBehaviour
{
    /**
     *  - Things to Change or add -
     *  Prone
     *  toggle Crouch
0     *  
     *  -------------------------------------------------------------------
     *  ==== Code By Dani, Dave, Me and Forum guys (: ====
     *  
     *  Copyright 2020/12/29 - Gabriel Spinola
     *  
     *  | free to use |
     *  -------------------------------------------------------------------
     * **/

    // Ready to jump removed may cause bugs in future.

    #region Variables

    private Rigidbody rb;
    private GlobalInputs inputs;
    private PlayerLook playerLook;

    [Header("References")]
    public LayerMask whatIsGround;
    public LayerMask whatIsWall;

    [Header("Config")]
    public float extraGravity;
    public float maxSlopeAngle;
    public bool grounded;

    [Header("Movement-Stats")]
    public float life;
    public float moveSpeed;
    public float maxSpeed;

    [Header("Counter Movement")]
    public float counterMovement;
    public float slideCounterMovement;

    private bool cancellingGrounded;

    private readonly float threshold = 0.01f;

    [Header("Jump-Stats")]
    public float jumpForce;
    public float jumpCoolDown = 0.25f;

    private Vector3 normalVector = Vector3.up;

    [Header("Crouch-Stats")]
    public float slideForce;
    public float slideMaxSpeed;

    private Vector3 crouchScale = new Vector3(1f, 0.5f, 1f);
    private Vector3 playerScale;

    [Header("WallRun-Stats")]
    public float wallRunGravity;
    public float wallrunForce;
    public float maxWallrunTime;
    public float maxWallrunSpeed;

    [HideInInspector]
    public bool isWallRight, isWallLeft, isWallRunning, isOnGround;
    public bool isCrouched;

    #endregion

    #region Run

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<GlobalInputs>();
        playerLook = GetComponent<PlayerLook>();
    }

    void Start()
    {
        playerScale = transform.localScale;
        isCrouched = false;
    }

    void FixedUpdate()
    {
        Movement();

        if (inputs.jumpKey)
            Jumping();
    }

    void Update()
    {
        CheckForWall();
        WallRunInput();

        bool isLocked = Physics.Raycast(transform.position, Vector3.up, 2f, whatIsGround);

        if (inputs.crouchKey && !isCrouched) {
            StartCrouch();

            isCrouched = true;
        }
        else if (inputs.crouchKey && isCrouched) {
            StopCrouch();

            isCrouched = false;
        }

        if (life <= 0)
            Die();
    }

    #endregion

    #region Movement

    /// <summary>
    /// Find the velocity relative to where the player is looking
    /// Useful for vectors calculations regarding movement and limiting movement
    /// </summary>
    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = playerLook.orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private void Movement()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * extraGravity);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x,
              yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(inputs.xAxis, inputs.zAxis, mag);

        float maxSpeed = this.maxSpeed;

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (inputs.xAxis > 0 && xMag > maxSpeed) inputs.xAxis = 0;
        if (inputs.xAxis < 0 && xMag <-maxSpeed) inputs.xAxis = 0;
        if (inputs.zAxis > 0 && yMag > maxSpeed) inputs.zAxis = 0;
        if (inputs.zAxis < 0 && yMag <-maxSpeed) inputs.zAxis = 0;

        //Some multipliers
        float multiplierH = 1f,
              multiplierV = 1f;

        // Movement in air
        if (!grounded) {
            multiplierH = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && inputs.crouchKey) {
            multiplierV = 0f;
        }

        if (!inputs.crouchKey) {
            // Apply forces to move player
            rb.AddForce(playerLook.orientation.transform.forward * inputs.zAxis * moveSpeed * Time.deltaTime * multiplierH * multiplierV);
            rb.AddForce(playerLook.orientation.transform.right * inputs.xAxis * moveSpeed * Time.deltaTime * multiplierH);
        } 
    }

    private void CounterMovement(float x, float z, Vector2 mag)
    { 
        float velX = 0;
        float velY = 0;

        if (inputs.keyLeft)  velX +=-1;
        if (inputs.keyRight) velX += 1;
        if (inputs.keyUp)    velY +=-1;
        if (inputs.keyDown)  velY += 1;

        float length = Mathf.Sqrt((Mathf.Pow(velX, 2)) + (Mathf.Pow(velY, 2)));

        if (length != 0) {
            velX /= length;
            velY /= length;
        }

        // Prevent Double Speed When Double Inputs
#pragma warning disable IDE0059 /// For Some Reason the Intellij is bugging here
        velX *= moveSpeed;
#pragma warning restore IDE0059

#pragma warning disable IDE0059
        velY *= moveSpeed;
#pragma warning restore IDE0059

        if (!grounded || inputs.jumpKey)
            return;

        // Counter Movement 
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
            rb.AddForce(moveSpeed * playerLook.orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);

        if (Math.Abs(mag.y) > threshold && Math.Abs(z) < 0.05f || (mag.y < -threshold && z > 0) || (mag.y > threshold && z < 0))
            rb.AddForce(moveSpeed * playerLook.orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);

        if (inputs.crouchKey) {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);

            return;
        }

        // Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2)) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;

            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    #endregion

    #region Jumping

    void Jumping()
    {
        if (grounded)
        {
            // Add Jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;

            if (rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            }
            else if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }
        }
    }

    #endregion

    #region Crouch

    void StartCrouch()
    {
        // Set crouch scale
        transform.localScale = crouchScale;

        rb.AddForce(playerLook.orientation.transform.forward * inputs.zAxis * slideForce * Time.deltaTime * 1);
        rb.AddForce(playerLook.orientation.transform.right * inputs.xAxis * slideForce * Time.deltaTime * 1);
    }

    void StopCrouch()
    {
        // Reset scale
        transform.localScale = playerScale;
    }

    #endregion

    #region WallRun

    void WallRunInput()
    {
        if (Input.GetKey(KeyCode.D) && isWallRight)
            StartWallrun();
        if (Input.GetKey(KeyCode.A) && isWallLeft)
            StartWallrun();
    }

    void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if (rb.velocity.magnitude <= maxWallrunSpeed && rb.velocity.magnitude >= 1f)
        {
            // Add forward Force
            rb.AddForce(playerLook.orientation.forward * wallrunForce * Time.deltaTime);

            // Add extra gravity
            rb.AddForce(new Vector3(0f, -wallRunGravity, 0f));

            //Make sure character sticks to wall
            if (isWallRight)
            {
                rb.AddForce(playerLook.orientation.right * wallrunForce / 5 * Time.deltaTime);
            }
            else
            {
                rb.AddForce(-playerLook.orientation.right * wallrunForce / 5 * Time.deltaTime);
            }
        }

        Invoke(nameof(StopWallRun), maxWallrunTime);
    }

    void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }

    void CheckForWall()
    {
        // Checkers
        isWallRight = Physics.Raycast(transform.position, playerLook.orientation.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -playerLook.orientation.right, 1f, whatIsWall);
        isOnGround = Physics.Raycast(transform.position, Vector3.down, 2f, whatIsWall);

        //leave wall run
        if ((!isWallLeft && !isWallRight) || isOnGround)
        {
            StopWallRun();
        }

        /// <breakpoint>
        /// Needs To be reworked
        /// </breakpoint>

        /*
        //reset double jump
        if ((isWallLeft || isWallRight) && inputs.jumpKey)
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }*/
    }

    #endregion

    #region Config

    void SlopeHandle(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);

        if (angle <= maxSlopeAngle && angle >= 0)
        {
            rb.AddForce(Vector3.down * 60f);
        }
    }

    public bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);

        return angle < maxSlopeAngle;
    }

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision col)
    {
        //Make sure we are only checking for walkable layers
        int layer = col.gameObject.layer;

        if (whatIsGround != (whatIsGround | (1 << layer)))
            return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < col.contactCount; i++)
        {
            Vector3 normal = col.contacts[i].normal;

            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                normalVector = normal;
                cancellingGrounded = false;

                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;

        if (!cancellingGrounded)
        {
            cancellingGrounded = true;

            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 12)
            Die();
    }

    private void StopGrounded() => grounded = false;

    private void Die() => SceneManager.LoadScene("SampleScene");

    #endregion
}