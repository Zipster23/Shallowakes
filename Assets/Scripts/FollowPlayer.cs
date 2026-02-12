using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -7);
    [SerializeField] private float cameraSensitivity;

    private float mouseXInput;
    private float mouseYInput;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = player.transform.position + offset;
        mouseXInput = Input.GetAxis("Mouse X");
        mouseYInput = Input.GetAxis("Mouse Y");
        //use transform.Rotate(-transform.up * rotateHorizontal * sensitivity) instead if you dont want the camera to rotate around the player
        transform.RotateAround(player.transform.position, Vector3.up, mouseXInput * cameraSensitivity);

        // again, use transform.Rotate(transform.right * rotateVertical * sensitivity) if you don't want the camera to rotate around the player
        transform.RotateAround(Vector3.zero, transform.right, mouseYInput * cameraSensitivity); 
    }
}
