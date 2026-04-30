using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SusanooSFXManager : MonoBehaviour
{

    [Header("Blade SFX")]
    [SerializeField] private AudioSource bladeSFX;
    [SerializeField] private AudioClip bladeAttackSFX;
    [SerializeField] private AudioClip bladeDeflectSFX;
    [SerializeField] private AudioClip bladeHitSFX;
    [SerializeField] private float bladeDeflectVolume = 0.7f;

    [Header("Misc SFX")]
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip enragedSFX;
    [SerializeField] private AudioClip playerDeathSFX;

    [Header("Ability SFX")]
    [SerializeField] private AudioClip lightningStrikeSFX;
    [SerializeField] private AudioClip windSlashSFX;

    public void PlayBladeAttackSFX()
    {
        bladeSFX.PlayOneShot(bladeAttackSFX);
    }

    public void PlayBladeDeflectSFX()
    {
        bladeSFX.PlayOneShot(bladeDeflectSFX, bladeDeflectVolume);
    }

    public void PlayBladeHitSFX()
    {
        bladeSFX.PlayOneShot(bladeHitSFX);
    }

    public void PlayDodgeSFX()
    {
        bladeSFX.PlayOneShot(dashSFX);
    }

    public void PlayEnragedSFX()
    {
        bladeSFX.PlayOneShot(enragedSFX);
    }

    public void PlayLightningStrikeSFX()
    {
        bladeSFX.PlayOneShot(lightningStrikeSFX);
    }

    public void PlayWindSlashSFX()
    {
        bladeSFX.PlayOneShot(windSlashSFX);
    }

    public void PlayerPlayerDeathSFX()
    {
        bladeSFX.PlayOneShot(playerDeathSFX);
    }


}
