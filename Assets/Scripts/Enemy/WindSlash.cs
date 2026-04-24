using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSlash : MonoBehaviour
{
   
    public float speed = 20f;       // how fast the projectile travels
    public int damage = 100;        // how much damage it deals
    public float lifetime = 5f;     // how long before it autodestroys
    public LayerMask playerLayer;   // to detect the player

    private Vector3 direction;      // direction the slash travels
    public bool isParriable = true; // can the player parry this




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
        
        // move forward every frame
        transform.position += direction * speed * Time.deltaTime;

        // check if player is hit
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 1f, playerLayer);
        foreach(Collider hit in hitPlayers)
        {
            PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

    }




    // called by PlayerParry when the player successfully parries this projectile
    public void GetParried()
    {
        
        Destroy(gameObject);

    }


}
