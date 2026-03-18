using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVFXManager : MonoBehaviour
{
    [SerializeField] private TrailRenderer swordTrail1;
    [SerializeField] private TrailRenderer swordTrail2;
    [SerializeField] private ParticleSystem parrySparks;


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

    public void EmitParryParticles()
    {
        parrySparks.Play();
    } 
}
