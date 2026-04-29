using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguSFXManager : MonoBehaviour
{

    [Header("Naginata SFX")]
    [SerializeField] private AudioSource naginataSFX;
    [SerializeField] private AudioClip naginataAttackSFX;
    [SerializeField] private AudioClip naginataDeflectSFX;
    [SerializeField] private AudioClip naginataHitSFX;
    [SerializeField] private float naginataDeflectVolume = 0.7f;

    [Header("Misc SFX")]
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip enragedSFX;

    public void PlayNaginataAttackSFX()
    {
        naginataSFX.PlayOneShot(naginataAttackSFX);
    }

    public void PlayNaginataDeflectSFX()
    {
        naginataSFX.PlayOneShot(naginataDeflectSFX, naginataDeflectVolume);
    }

    public void PlayNaginataHitSFX()
    {
        naginataSFX.PlayOneShot(naginataHitSFX);
    }

    public void PlayDodgeSFX()
    {
        naginataSFX.PlayOneShot(dashSFX);
    }

    public void PlayEnragedSFX()
    {
        naginataSFX.PlayOneShot(enragedSFX);
    }
}
