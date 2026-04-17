using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public bool attackInput;
    private float attackInputBufferTime = 0.1f;
    private float attackInputTimer = 0f;
    public bool thrustInput;
    public bool parryInput;
    public bool jumpInput;
    public bool sprintInput;
    public bool dashInput;
    public bool glideInput;

    private void Update()
    {
        MovementInput = new Vector2(
        Input.GetAxis("Horizontal"),
        Input.GetAxis("Vertical")
        );

        // When the player presses mouse0, start the buffer timer
        if (Input.GetKeyDown(KeyCode.Mouse0))
            attackInputTimer = attackInputBufferTime;

        // Count the timer down every frame
        if (attackInputTimer > 0)
            attackInputTimer -= Time.deltaTime;

        // attackInput stays true for attackInputBufferTime seconds after the keypress
        // instead of just one frame — this is what makes buffering reliable
        attackInput = attackInputTimer > 0;

        thrustInput = Input.GetKeyDown(KeyCode.Mouse1);
        parryInput = Input.GetKeyDown(KeyCode.F);
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        sprintInput = Input.GetKey(KeyCode.LeftShift);
        dashInput = Input.GetKeyDown(KeyCode.Q);
        glideInput = Input.GetKey(KeyCode.G);
    }
}