using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float turnSpeed = 10f;

    private Transform camTransform;

    private void Awake()
    {
        camTransform = Camera.main.transform;
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

        if (moveDirection.magnitude > 0.1f)
        {
            transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}

