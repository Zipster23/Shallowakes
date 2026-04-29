using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SusanooVFXManager : MonoBehaviour
{

    [Header("Blade VFX")]
    [SerializeField] private TrailRenderer bladeTrail1;
    [SerializeField] private TrailRenderer bladeTrail2;
    [SerializeField] private ParticleSystem parrySparks;
    [SerializeField] private ParticleSystem swingBurst;

    [Header("Misc VFX")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;

    [Header("Enraged VFX")]
    [SerializeField] private ParticleSystem enragedEffect;
    [SerializeField] private ParticleSystem lightningAura;


    [Header("Dash-Ability VFX")]
    [SerializeField] private ParticleSystem slashChargeUpEffect;
    [SerializeField] private ParticleSystem slashEffect;

    [Header("Cinematic VFX")]
    [SerializeField] private ParticleSystem appearSmokeEffect;
    

    public void StartSwingEffects()
    {
        bladeTrail1.emitting = true;
        bladeTrail2.emitting = true;
        swingBurst.Play();
    }

    public void StopSwingEffects()
    {
        bladeTrail1.emitting = false;
        bladeTrail2.emitting = false;
    }

    public void EmitSparkParticles()
    {
        parrySparks.Play();
    } 

    public void PlayHitEffect(Vector3 position)
    {
        ParticleSystem effect = Instantiate(hitEffect, position, Quaternion.identity);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayDodgeEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(dashEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent); // attach to the tengu so that the effect follows
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlaySlashEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(slashEffect, position, slashEffect.transform.rotation);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayEnragedEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(enragedEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayLightningAuraEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(lightningAura, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }



    
}
