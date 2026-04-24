using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TenguAI;

public class DashSlash : MonoBehaviour
{
    public Transform player;
    private Animator animator;

    private bool isDashSlashing = false;
    public float attackRange = 3f;
    public float dashSlashSpeed = 80f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleDashSlash();
    }

    private void HandleDashSlash()
    {

        // Only start the sequence if we aren't already in the middle of it
        if (!isDashSlashing)
        {
            StartCoroutine(DashSlashSequence());
        }

    }

    private IEnumerator DashSlashSequence()
    {
        // flag that the dash slash is in progress so HandleDashSlash doesn't restart it
        isDashSlashing = true;

        // trigger the dash slash animation in the Animator
        animator.SetTrigger("DashSlash");

        // let the first 30% of the animation play naturally
        // this is the charge up stance before the dash
        // normalizedTime goes from 0 to 1 as the animation plays, so 0.3 = 30% through
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
        {
            // keep facing the player during the stance
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            yield return null;
        }

        // freeze the dash slash animation by setting its speed to 0
        // this pauses it on the dash pose while we move the Tengu forward
        // DashSlashSpeed is a float parameter in the Animator that controls only this animation's speed
        animator.SetFloat("DashSlashSpeed", 0f);

        // move towards the player while the animation is frozen
        // keep moving every frame until the Tengu is within attack range
        while (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            // move towards the player at dashSlashSpeed
            transform.position = Vector3.MoveTowards(transform.position, player.position, dashSlashSpeed * Time.deltaTime);
            // keep facing the player while dashing
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
            yield return null;
        }

        // resume the animation at normal speed to play the actual slash
        animator.SetFloat("DashSlashSpeed", 1f);

        // wait for the animation to reach 95% before switching states
        // this prevents snapping into a different animation mid-slash
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            yield return null;
        }

        // dash slash is done, reset the flag and go back to Attack state
        isDashSlashing = false;
    }
}
