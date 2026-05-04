using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField]private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // 1. Calculate the direction from the player to this object
            // (Subtracting player from self gives the vector pointing AWAY)
            Vector3 directionAway = transform.position - player.transform.position;

            // 2. Ignore the height (Y axis) so it only rotates on the X and Z
            directionAway.y = 0;

            // 3. Check to ensure the direction isn't zero (prevents console errors)
            if (directionAway != Vector3.zero)
            {
                // 4. Create a rotation based on that direction
                transform.rotation = Quaternion.LookRotation(directionAway);
            }
        }
    }
}
