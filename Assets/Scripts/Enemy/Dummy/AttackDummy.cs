using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDummy : MonoBehaviour
{
    private YokaiVFXManager vfx;
    private void Start()
    {
        vfx = GetComponent<YokaiVFXManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            vfx.PlayDeathEffect(transform.position, gameObject.transform);
            gameObject.SetActive(false);
        }
    }
}
