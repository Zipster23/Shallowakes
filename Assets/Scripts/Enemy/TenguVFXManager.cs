using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguVFXManager : MonoBehaviour
{
    [SerializeField] private TrailRenderer swordTrail1;
    [SerializeField] private TrailRenderer swordTrail2;
    [SerializeField] private ParticleSystem parrySparks;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem magicCircleEffect;
    [SerializeField] private ParticleSystem redCloudEffect;
    

    public void StartSwingEffects()
    {
        swordTrail1.emitting = true;
        swordTrail2.emitting = true;
    }

    public void StopSwingEffects()
    {
        swordTrail1.emitting = false;
        swordTrail2.emitting = false;
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

    public void PlayRedCloudEffect(Vector3 position, Transform parent)
    {
        ParticleSystem effect = Instantiate(redCloudEffect, position, Quaternion.identity);
        effect.transform.SetParent(parent);
        Destroy(effect.gameObject, effect.main.duration);
    }
}
