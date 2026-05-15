using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDummy : MonoBehaviour
{
    private YokaiVFXManager vfx;
    private Enemy enemy;
    private void Start()
    {
        vfx = GetComponent<YokaiVFXManager>();
        enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        if (enemy.currentHealth <= 0)
        {
            vfx.PlayDeathEffect(transform.position, gameObject.transform);
            gameObject.SetActive(false);
        }
    }
}
