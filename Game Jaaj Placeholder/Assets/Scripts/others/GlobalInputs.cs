using UnityEngine;

public class GlobalInputs : MonoBehaviour
{
    [Header("Movement")]
    public float xAxis;
    public float zAxis;

    public bool jumpKey;
    public bool crouchKey;

    [Header("Counter Movement")]
    public bool keyRight;
    public bool keyLeft;
    public bool keyUp;
    public bool keyDown;


    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        zAxis = Input.GetAxisRaw("Vertical");

        jumpKey = Input.GetButton("Jump") || Input.GetButtonDown("Jump");
        crouchKey = Input.GetKeyDown(KeyCode.LeftControl);

        keyRight = Input.GetKey(KeyCode.D);
        keyLeft = Input.GetKey(KeyCode.A);
        keyDown = Input.GetKey(KeyCode.S);
        keyUp = Input.GetKey(KeyCode.W);
    }
}
