using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public bool attackInput;
    public bool thrustInput;
    public bool parryInput;
    public bool jumpInput;

    private void Update()
    {
        MovementInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        // Attack input
        attackInput = Input.GetKeyDown(KeyCode.Mouse0);

        thrustInput = Input.GetKeyDown(KeyCode.Mouse1);

        parryInput = Input.GetKeyDown(KeyCode.F);

        // Jump Input
        jumpInput = Input.GetKeyDown(KeyCode.Space);
    }
}

