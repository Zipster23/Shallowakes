using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidDeath : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    private void OnTriggerEnter(Collider other)
    {
        playerHealth.TakeDamage(1000);
    }
}
