using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstCamera : MonoBehaviour
{
    public float turnSpeed = 4.0f;
    public float moveSpeed = 2.0f;
    
    public float minTurnAngle = -90.0f;
    public float maxTurnAngle = 90.0f;
    private float rotX;
    
    void Update ()
    {
        MouseAiming();
        KeyboardMovement();
    }
    
    void MouseAiming ()
    {
        if(Input.GetKey(KeyCode.LeftControl)) {
            return;
        }

        // get the mouse inputs
        float y = Input.GetAxis("Mouse X") * turnSpeed;
        rotX += Input.GetAxis("Mouse Y") * turnSpeed;
    
        // clamp the vertical rotation
        rotX = Mathf.Clamp(rotX, minTurnAngle, maxTurnAngle);
    
        // rotate the camera
        transform.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);
    }
    
    void KeyboardMovement ()
    {
        Vector3 dir = new Vector3(0, 0, 0);
    
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");

        Vector3 camera2DForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 camera2DRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 translation = moveSpeed * (camera2DForward * Input.GetAxis("Vertical") + camera2DRight * Input.GetAxis("Horizontal"));

        transform.Translate(translation * Time.deltaTime, Space.World);
    }
}
