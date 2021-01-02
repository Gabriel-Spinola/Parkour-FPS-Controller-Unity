using System;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    Player player;

    [Header("References")]
    public Transform playerCam;
    public Transform orientation;

    [Header("Config")]
    public float sesitivity;

    [Header("Wall Run Camera Influence")]
    public float maxWallRunCameraTilt;

    private float wallRunCameraTilt;
    private float xRotation;
    private float desiredX;


    [Header("Leaning")]
    public float maxLeaningAngle;

    public float currAngle;
    public bool isLeaningRight;
    public bool isLeaningLeft;

    void Awake() => player = GetComponent<Player>();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sesitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sesitivity * Time.fixedDeltaTime;

        // Find Current look Rotation
        Vector2 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        // Rotate And Make Sure we don't over- or under-rotate
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        currAngle = playerCam.transform.localRotation.eulerAngles.z;

        // Perform the Rotation
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX,0);

        if (player.isWallRunning)
        { 
            playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        } 
        else
        {
            if (Input.GetKey(KeyCode.Q)) {
                isLeaningLeft = true;

                playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 30f, 30f * Time.deltaTime));
            }
            else if (Input.GetKey(KeyCode.E)) {
                isLeaningRight = true;

                playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, -30f, 30f * Time.deltaTime));
            }
            else {
                isLeaningLeft = false;
                isLeaningRight = false;

                playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 0f, 40f * Time.deltaTime));
            }

            if (currAngle <= 30)
                Debug.Log("Left");

            if (currAngle <= 330 && currAngle > 30)
                Debug.Log("Right");
        }

        WallRunTiltManager();
    }

    void WallRunTiltManager()
    {
        ///<summary>
        /// Debug
        /// </summary>
        if (player.isOnGround || (player.isWallLeft && player.isWallRight))
        {
            player.isWallRunning = false;
            player.isWallLeft = false;
            player.isWallRight = false;
        }
        else if (player.isWallRight || player.isWallLeft)
        {      
            player.isWallRunning = true;
        }

        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && player.isWallRunning && player.isWallRight)
        {
            // Add camera tilt in right angle
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }

        if (Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && player.isWallRunning && player.isWallLeft)
        {
            // Add camera tilt in left angle
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }

        if (wallRunCameraTilt > 0 && !player.isWallRight && !player.isWallLeft)
        {
            // Reset camera angle (right)
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }

        if (wallRunCameraTilt < 0 && !player.isWallRight && !player.isWallLeft)
        {
            // Reset camera angle (left)
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }
    }
}
/*
 * void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sesitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sesitivity * Time.fixedDeltaTime;

        // Find Current look Rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        // Rotate And Make Sure we don't over- or under-rotate
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        currAngle = playerCam.transform.localRotation.eulerAngles.z;

        // Perform the Rotation
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
        if (player.isWallRunning)
        { 
            playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        } 
        else
        {
            // playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 30f, 30f * Time.deltaTime));
            // playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, -30f, 30f * Time.deltaTime));

            if (Input.GetKeyDown(KeyCode.Q) && !isLeaningLeft) {
                StartCoroutine(LeanLeft());
            }

            if (Input.GetKeyDown(KeyCode.E) && !isLeaningRight) {
                StartCoroutine(LeanRight());
            }

            //playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 0f, 40f * Time.deltaTime));
        }

        WallRunTiltManager();
    }

    IEnumerator LeanRight()
    {
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, -30f, 30f * Time.deltaTime));
        isLeaningRight = true;

        yield return new WaitForSeconds(10f);

        if (Input.GetKeyDown(KeyCode.Q) && isLeaningLeft) {
            StartCoroutine(LeanBack());
        }
    }
    
    IEnumerator LeanLeft()
    {
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 30f, 30f * Time.deltaTime));
        isLeaningLeft = true;

        yield return new WaitForSeconds(10f);

        if (Input.GetKeyDown(KeyCode.E) && isLeaningRight) {
            StartCoroutine(LeanBack());
        }
    }

    IEnumerator LeanBack()
    {
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, Mathf.MoveTowardsAngle(currAngle, 0f, 40f * Time.deltaTime));
        isLeaningRight = false;
        isLeaningLeft = false;

        yield return new WaitForSeconds(10f);
    }
*/