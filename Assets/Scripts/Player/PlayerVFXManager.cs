using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private TrailRenderer swordTrail1;
    [SerializeField] private TrailRenderer swordTrail2;
    [SerializeField] private ParticleSystem swingBurst;
    [SerializeField] private ParticleSystem parrySparks;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;
    


    public void StartSwingEffects()
    {
        swordTrail1.emitting = true;
        swordTrail2.emitting = true;
        swingBurst.Play();
    }

    public void StopSwingEffects()
    {
        swordTrail1.emitting = false;
        swordTrail2.emitting = false;
    }
    
    public void ForceStopSwingEffects()
    {
        swordTrail1.emitting = false;
        swordTrail2.emitting = false;
        // also clear any existing trail data
        swordTrail1.Clear();
        swordTrail2.Clear();
    }
    
    public void EmitParryParticles()
    {
        parrySparks.Play();
    } 

    public void PlayHitEffect(Vector3 position)
    {
        ParticleSystem effect = Instantiate(hitEffect, position, Quaternion.identity);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayDashEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(dashEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent); // attach to the player so that the effect follows
        Destroy(effect.gameObject, effect.main.duration);
    }

    
}
