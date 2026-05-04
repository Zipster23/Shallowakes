using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private TrailRenderer swordTrail1;
    [SerializeField] private TrailRenderer swordTrail2;
    [SerializeField] private ParticleSystem swingBurst;
    [SerializeField] private ParticleSystem parrySparks;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private ParticleSystem getHitEffect;

    [Header("Scene Settings")]
    [SerializeField] private string tutorialSceneName = "Tutorial";

    [SerializeField] private TrailRenderer sprintTrail; // assign in Inspector

    public void SetSprintTrail(bool isActive)
    {
        if (sprintTrail != null)
            sprintTrail.emitting = isActive;
    }

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

    public void PlayGetHitVFX(Vector3 position)
    {
        if (SceneManager.GetActiveScene().name != tutorialSceneName) return;

        if (getHitEffect == null)
        {
            Debug.LogWarning("PlayerVFXManager: getHitEffect is not assigned.");
            return;
        }

        ParticleSystem effect = Instantiate(getHitEffect, position, Quaternion.identity);
        Destroy(effect.gameObject, effect.main.duration);
    }
}
