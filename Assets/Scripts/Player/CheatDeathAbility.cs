using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatDeathAbility : MonoBehaviour
{
    public bool canCheatDeath = true;
    [SerializeField] private int healthAfterRevive = 1;

    // References
    private PlayerHealth health;
    private PlayerAnimatorController playerAnimatorController;
    private PlayerVFXManager playerVFXManager;
    private PlayerSFXManager playerSFXManager;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        playerVFXManager = GetComponent<PlayerVFXManager>();
        playerSFXManager = GetComponent<PlayerSFXManager>();
        
    }

    public void Activate()
    {
        if (canCheatDeath)
        {
            canCheatDeath = false;
            health.currentHealth = healthAfterRevive;

            playerVFXManager.PlayReviveParticles();
            playerSFXManager.PlayReviveSFX();

            // playerAnimatorController.PlayReviveAnimation(); <- Add Back after KO contains an animator controller + revive animation
            // Also make sure to create the CheatingDeath Trigger in parameters

        }
    }
}
