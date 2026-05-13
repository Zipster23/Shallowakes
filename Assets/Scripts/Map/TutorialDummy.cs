using UnityEngine;
using System;

public class TutorialDummy : MonoBehaviour
{
    public enum TutorialAction
    {
        Parry,
        Attack
    }

    [Header("Tutorial Settings")]
    [SerializeField] private YokaiAI yokaiAI;
    [SerializeField] private Enemy enemyHealth;

    private TutorialAction requiredAction;
    private Action onActionCompleted;
    private bool actionCompleted = false;

    private void Awake()
    {
        yokaiAI = GetComponent<YokaiAI>();
        enemyHealth = GetComponent<Enemy>();
    }

    public void Initialize(TutorialAction action, Action completionCallback)
    {
        requiredAction = action;
        onActionCompleted = completionCallback;
        actionCompleted = false;

        // Make the dummy invincible
        if (enemyHealth != null)
        {
            enemyHealth.isInvincible = true;
        }

        // Make the dummy deal 0 damage
        if (yokaiAI != null)
        {
            yokaiAI.attackDamage = 0;
            yokaiAI.currentState = YokaiAI.YokaiState.Chase;

            // For attack dummy, disable its ability to parry/dodge
            if (requiredAction == TutorialAction.Attack)
            {
                yokaiAI.parryChance = 0;
                yokaiAI.dodgeChance = 0;
            }
        }
    }

    // Called by PlayerParry when player successfully parries
    public void OnParried()
    {
        if (actionCompleted) return;

        if (requiredAction == TutorialAction.Parry)
        {
            actionCompleted = true;
            onActionCompleted?.Invoke();
        }
    }

    // Called by player weapon when attack connects
    public void OnHitByPlayer()
    {
        if (actionCompleted) return;

        if (requiredAction == TutorialAction.Attack)
        {
            actionCompleted = true;
            onActionCompleted?.Invoke();
        }
    }
}