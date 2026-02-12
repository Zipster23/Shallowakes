using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset = new Vector3(0, 5, -7);

    private float mouseXInput;
    private float mouseYInput;

    // Update is called once per frame
    void LateUpdate()
    {
        mouseXInput = Input.GetAxis("Mouse X");
        mouseYInput = Input.GetAxis("Mouse Y");
        transform.position = player.transform.position + offset;
    }
}
