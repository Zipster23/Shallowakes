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

    private void Awake()
    {
        camTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    public void Move(Vector2 input)
    {
        if (camTransform == null) return;

        float horizontalInput = input.x;
        float forwardInput = input.y;

        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * forwardInput) + (camRight * horizontalInput);

        CurrentSpeed = moveDirection.magnitude;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition = rb.position + moveDirection.normalized * speed * Time.deltaTime;
            rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.deltaTime));
        }
    }
}

