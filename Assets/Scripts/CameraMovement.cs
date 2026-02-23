using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Transform camTransform;

    public void Awake()
    {
        camTransform = Camera.main.transform;
    }

    public void CameraMove()
    {
        // if theres no camera don't move
        if (camTransform == null) return;

        // references to camera direction
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;

        // Used to prevent the player from flying by looking up
        camForward.y = 0;
        camRight.y = 0;

        // prevent the player from moving faster diagonally
        camForward.Normalize();
        camRight.Normalize();
    }
}
