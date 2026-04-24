using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokaiClone : MonoBehaviour
{
    private void Awake()
    {
        // make the clone darker to distinguish from the real enemy
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = new Color(0.3f, 0.3f, 0.3f, 0.1f);
        }

        // disable ShadowClone so the clone doesn't start spawning its own clones
        ShadowClone sc = GetComponent<ShadowClone>();
        if (sc != null) sc.enabled = false;
    }
}