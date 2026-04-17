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
    public bool sprintInput;
    public bool dashInput;

    private PauseManager pauseManager; // stored reference to PauseManager

    void Start()
    {
        // find PauseManager once at startup instead of searching every frame
        pauseManager = FindObjectOfType<PauseManager>();
    }

    private void Update()
    {
        // Ensures not inputs can be made for movement while paused
        if (!pauseManager.paused)
        {
            MovementInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
            );

            // Attack inputs
            attackInput = Input.GetKeyDown(KeyCode.Mouse0);

            // Thrust Input
            thrustInput = Input.GetKeyDown(KeyCode.Mouse1);

            // Parry Input
            parryInput = Input.GetKeyDown(KeyCode.F);

            // Jump Input
            jumpInput = Input.GetKeyDown(KeyCode.Space);

            // Sprint Input
            sprintInput = Input.GetKey(KeyCode.LeftShift);

            dashInput = Input.GetKeyDown(KeyCode.Q);
        }

        
        // Pause Input
        if (Input.GetKeyDown(KeyCode.P) && pauseManager != null)
            pauseManager.TogglePause();
    }
}