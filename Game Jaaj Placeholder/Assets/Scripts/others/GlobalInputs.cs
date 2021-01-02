using UnityEngine;

public class GlobalInputs : MonoBehaviour
{
    public float xAxis, zAxis;
    public bool jumpKey, crouchKey, keyRight, keyLeft, keyUp, keyDown;

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
