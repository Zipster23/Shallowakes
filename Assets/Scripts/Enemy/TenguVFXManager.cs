using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguVFXManager : MonoBehaviour
{

    [Header("Naginata VFX")]
    [SerializeField] private TrailRenderer naginataTrail1;
    [SerializeField] private TrailRenderer naginataTrail2;
    [SerializeField] private ParticleSystem parrySparks;

    [Header("Misc VFX")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;

    [Header("Enraged Mode VFX")]
    [SerializeField] private ParticleSystem magicCircleEffect;
    [SerializeField] private ParticleSystem redRayEffect;

    [Header("Dash-Ability VFX")]
    [SerializeField] private ParticleSystem slashChargeUpEffect;
    [SerializeField] private ParticleSystem slashEffect;
    

    public void StartSwingEffects()
    {
        naginataTrail1.emitting = true;
        naginataTrail2.emitting = true;
    }

    public void StopSwingEffects()
    {
        naginataTrail1.emitting = false;
        naginataTrail2.emitting = false;
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

    public void PlayMagicCircleEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(magicCircleEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayRedRayEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(redRayEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlaySlashChargeUpEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(slashChargeUpEffect, position, slashEffect.transform.rotation);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlaySlashEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(slashEffect, position, slashEffect.transform.rotation);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }


    
}
