using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float turnSpeed = 10.0f;

    private Transform camTransform;

    void Start()
    {
        // Locks the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;

        // Makes the cursor invisible
        Cursor.visible = false;

        // Existing camera reference
        camTransform = Camera.main.transform;

        // Automatically find the camera tagged "MainCamera"
        camTransform = Camera.main.transform;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float forwardInput = Input.GetAxis("Vertical");

        // 1. Get the direction the camera is facing
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // 2. IMPORTANT: Flatten the directions so the player doesn't move up/down
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. Calculate the move direction relative to the camera
        Vector3 moveDirection = (camForward * forwardInput) + (camRight * horizontalInput);

        if (moveDirection.magnitude > 0.1f)
        {
            // 4. Move the player in World Space
            transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);

            // 5. Rotate the player to face the way they are moving
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Unlocks and shows the cursor if you hit Escape
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

