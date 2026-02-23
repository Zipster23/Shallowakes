using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float turnSpeed = 10f;

    public float CurrentSpeed { get; private set; }

    private Transform camTransform;

    private Rigidbody rb;

    private Vector3 moveDir;

    private void Awake()
    {
        camTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 input)
    {
        // if theres no camera don't move
        if (camTransform == null) return;

        float horizontalInput = input.x;
        float forwardInput = input.y;

        // references to camera direction
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // Used to prevent the player from flying by looking up
        camForward.y = 0;
        camRight.y = 0;

        // prevent the player from moving faster diagonally
        camForward.Normalize();
        camRight.Normalize();

        // consolidate direcetion + input into one vector
        Vector3 moveDirection = (camForward * forwardInput) + (camRight * horizontalInput);
        
        CurrentSpeed = moveDirection.magnitude;

        // do ctrl + / while selecting the block to uncomment
        // // if the player has a move direction (is pressing the movement keys)
        // if (moveDirection.magnitude > 0.1f)
        // {
        //     // transform the player based on move direction and relative to the world, not the players transform
        //     // this allows the player to mvoe with respect to camera
        //     // THIS IS THE LINE TO CHANGE TO Rigidbody.MovePosition to prevent jitters.
        //     // BUT Antonio figure this part out please.
        //     // transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);

        //     rb.AddForce(moveDirection.normalized * speed);

        //     // Gets the direction in which the player is moving
        //     Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        //     // Rotates player in that direction
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        // }

        moveDir = moveDirection;
    }

    void FixedUpdate()
    {
        if (moveDir.magnitude > 0.1f)
        {
            rb.AddForce(moveDir.normalized * speed);
        }
        
    }
}

