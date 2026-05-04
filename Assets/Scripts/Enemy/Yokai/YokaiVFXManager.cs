using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokaiVFXManager : MonoBehaviour
{
    [Header("Weapon VFX")]
    [SerializeField] private TrailRenderer weaponTrail1;
    [SerializeField] private TrailRenderer weaponTrail2;
    [SerializeField] private ParticleSystem parrySparks;
    [SerializeField] private ParticleSystem swingBurst;

    [Header("Misc VFX")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;

    [Header("Shadow Clone Ability VFX")]
    [SerializeField] private ParticleSystem shadowCloneEffect;

    [Header("Cinematic VFX")]
    [SerializeField] private ParticleSystem appearSmokeEffect;
    [SerializeField] private ParticleSystem deathEffect;


    // --- Weapon Trail & Swing ---

    public void StartSwingEffects()
    {
        if(weaponTrail1 != null) weaponTrail1.emitting = true;
        if(weaponTrail2 != null) weaponTrail2.emitting = true;
        if(swingBurst   != null) swingBurst.Play();
    }

    public void StopSwingEffects()
    {
        if(weaponTrail1 != null) weaponTrail1.emitting = false;
        if(weaponTrail2 != null) weaponTrail2.emitting = false;
    }


    // --- Parry ---

    public void EmitSparkParticles()
    {
        if(parrySparks != null) parrySparks.Play();
    }


    // --- Hit ---

    public void PlayHitEffect(Vector3 position)
    {
        if(hitEffect == null) return;
        ParticleSystem effect = Instantiate(hitEffect, position, Quaternion.identity);
        Destroy(effect.gameObject, effect.main.duration);
    }


    // --- Dodge ---

    public void PlayDodgeEffect(Vector3 position, Transform parent)
    {
        if(dashEffect == null) return;
        ParticleSystem effect = Instantiate(dashEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }


    // --- Shadow Clone Ability ---

    public void PlayShadowCloneEffect(Vector3 position, Transform parent)
    {
        if(shadowCloneEffect == null) return;
        ParticleSystem effect = Instantiate(shadowCloneEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }


    // --- Cinematic ---

    public void PlayAppearSmokeEffect(Vector3 position, Transform parent)
    {
        if(appearSmokeEffect == null) return;
        ParticleSystem effect = Instantiate(appearSmokeEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }

    public void PlayDeathEffect(Vector3 position, Transform parent)
    {
        if (deathEffect == null) return;
        ParticleSystem effect = Instantiate(deathEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }
}
