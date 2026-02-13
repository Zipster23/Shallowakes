using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float speed = 20.0f;
    [SerializeField] private float turnSpeed = 45.0f;
    private float horizontalInput;
    private float forwardInput;
    private Rigidbody playerRb;
    public float jumpForce;
    public float gravityModifier;
    public bool isOnGround = true;

    void Start()
    {

        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;

    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        // Moves the car forward based on vertical input
        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        // Rotates the car based on horizontal input
        transform.Rotate(Vector3.up * Time.deltaTime * turnSpeed * horizontalInput);

        if(Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isOnGround = true;
    }

}
