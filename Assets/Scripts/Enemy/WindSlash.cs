using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSlash : MonoBehaviour
{
   
    public float speed = 20f;       // how fast the projectile travels
    public int damage = 100;        // how much damage it deals
    public float lifetime = 5f;     // how long before it autodestroys
    public LayerMask playerLayer;   // to detect the player

    [SerializeField] private SusanooVFXManager vfx;    // handles hit visual effects
    [SerializeField] private SusanooSFXManager sfx;    // handles hit sound effects

    private Vector3 direction;      // direction the slash travels
    public bool isParriable = true; // can the player parry this

    public enum SlashType { Vertical, Horizontal }
    public SlashType slashType = SlashType.Vertical;



    private void Start()
    {
        
        // auto destroy after lifetime seconds so it doesn't fly forever
        Destroy(gameObject, lifetime);

    }




    public void SetDirection(Vector3 dir)
    {
        
        direction = dir.normalized;

    }    




    private void Update()
    {
        
        Vector3 moveStep = direction * speed * Time.deltaTime;

        // SphereCast in the direction of movement so fast projectiles can't skip through player
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, 1.5f, direction, out hit, moveStep.magnitude, playerLayer))
        {
            PlayerHealth playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            if(playerHealth != null)
            {
                if(slashType == WindSlash.SlashType.Horizontal)
                {
                    PlayerMovement pm = hit.collider.GetComponentInParent<PlayerMovement>();
                    if(pm != null && !pm.isGrounded)
                    {
                        // player jumped over it, just move normally
                        transform.position += moveStep;
                        return;
                    }
                }
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        
            // no hit this frame, keep moving
            transform.position += moveStep;

        }

    




    // called by PlayerParry when the player successfully parries this projectile
    public void GetParried()
    {
        
        Destroy(gameObject);

    }


}
