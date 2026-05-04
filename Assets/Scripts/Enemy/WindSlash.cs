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
        
        transform.position += direction * speed * Time.deltaTime;

        // use OverlapBox instead of OverlapSphere to cover the full slash width
        Collider[] hitPlayers = Physics.OverlapBox(
            transform.position,
            transform.localScale / 2f, // half extents based on slash scale
            transform.rotation,
            playerLayer
        );

        foreach(Collider hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if(playerHealth != null)
            {
                if(slashType == WindSlash.SlashType.Horizontal)
                {
                    PlayerMovement pm = hit.GetComponentInParent<PlayerMovement>();
                    if(pm != null && !pm.isGrounded) return;
                }
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
            }

            // slightly adjust direction towards player every frame
            if(playerHealth != null)
            {
                Vector3 toPlayer = (playerHealth.transform.position - transform.position).normalized;
                direction = Vector3.Lerp(direction, toPlayer, 0.5f * Time.deltaTime);
            }
            transform.position += direction * speed * Time.deltaTime;
        }

    }

    




    // called by PlayerParry when the player successfully parries this projectile
    public void GetParried()
    {
        
        Destroy(gameObject);

    }


}
