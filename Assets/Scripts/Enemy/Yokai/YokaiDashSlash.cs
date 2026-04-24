using System.Collections;
using UnityEngine;

/// <summary>
/// Example ability — Yokai dashes across the arena and slashes the player.
/// Attach to the same GameObject as YokaiAI.
/// Mirrors TenguAI's DashSlash logic but fully decoupled from base AI.
/// </summary>
public class YokaiDashSlash : YokaiAbility
{
    [Header("Dash Slash Settings")]
    public float triggerRange   = 20f;  // Yokai only triggers this if player is this far away
    public float dashSpeed      = 80f;  // how fast the Yokai moves during the dash
    public int   triggerChance  = 100;  // % chance to trigger when conditions are met (set lower if other abilities compete)

    private bool isDashSlashing = false;


    // --- YokaiAbility overrides ---

    public override bool TryTrigger(YokaiAI ai)
    {
        // Only trigger if player is beyond the trigger range
        float distance = Vector3.Distance(transform.position, ai.player.position);
        if(distance < triggerRange) return false;

        // Roll for chance
        if(Random.Range(0, 100) >= triggerChance) return false;

        // Fire
        StartCoroutine(DashSlashSequence(ai));
        return true;
    }

    public override bool OnYokaiParried()
    {
        // Allow default parry stun to apply — return false
        isDashSlashing = false;
        StopAllCoroutines();
        return false;
    }

    public override void OnInterrupted()
    {
        isDashSlashing = false;
        StopAllCoroutines();
        animator.SetFloat("DashSlashSpeed", 1f);
    }


    // --- Ability sequence ---

    private IEnumerator DashSlashSequence(YokaiAI ai)
    {
        isDashSlashing = true;

        animator.SetTrigger("DashSlash");

        // Wait until the Animator has actually entered the DashSlash state
        while(!animator.GetCurrentAnimatorStateInfo(0).IsName("DashSlash"))
        {
            yield return null;
        }

        // Let the first 30% play (charge-up pose), keep facing the player
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
        {
            ai.FacePlayer();
            yield return null;
        }

        // Freeze the animation while we move the Yokai forward
        animator.SetFloat("DashSlashSpeed", 0f);

        while(Vector3.Distance(transform.position, ai.player.position) > ai.attackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, ai.player.position, dashSpeed * Time.deltaTime);
            ai.FacePlayer();
            yield return null;
        }

        // Resume the animation to play the actual slash
        animator.SetFloat("DashSlashSpeed", 1f);

        // Wait for the slash to nearly finish before handing control back
        while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }

        // Done — hand control back to YokaiAI
        isDashSlashing = false;
        ai.NotifyAbilityComplete();
    }
}
