using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YokaiSFXManager : MonoBehaviour
{
    [Header("Weapon SFX")]
    [SerializeField] public AudioSource yokaiAudioSource;
    [SerializeField] private AudioClip AttackSFX;
    [SerializeField] private AudioClip DeflectSFX;
    [SerializeField] private AudioClip weaponHitSFX;
    [SerializeField] private float weaponDeflectVolume = 0.7f;

    [Header("Misc SFX")]
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip appearSFX;
    [SerializeField] public AudioClip deathSFX;
    [SerializeField] private float deathSFXVolume = 0.5f;


    public void PlayWeaponAttackSFX()
    {
        yokaiAudioSource.PlayOneShot(AttackSFX);
    }

    public void PlayWeaponDeflectSFX()
    {
        yokaiAudioSource.PlayOneShot(weaponHitSFX, weaponDeflectVolume);
    }

    public void PlayWeaponHitSFX()
    {
        yokaiAudioSource.PlayOneShot(weaponHitSFX);
    }

    public void PlayDodgeSFX()
    {
        yokaiAudioSource.PlayOneShot(dashSFX);
    }

    public void PlayAppearSFX()
    {
        yokaiAudioSource.PlayOneShot(appearSFX);
    }

    public void PlayDeathSFX()
    {
        AudioSource.PlayClipAtPoint(deathSFX, transform.position, deathSFXVolume);
    }
}
