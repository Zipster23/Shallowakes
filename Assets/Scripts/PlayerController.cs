using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerInputHandler input;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        movement.Move(input.MovementInput);
    }
}


