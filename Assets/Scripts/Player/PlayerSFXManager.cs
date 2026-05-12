using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource katanaSFX;
    [SerializeField] private AudioClip katanaAttackSFX;
    [SerializeField] private AudioClip katanaDeflectSFX;
    [SerializeField] private AudioClip katanaHitSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip getHitSFX;
    [SerializeField] private AudioClip projectileSlashSFX;
    [SerializeField] private float katanaDeflectVolume = 0.7f;

    public void playKatanaAttackSFX()
    {
        katanaSFX.PlayOneShot(katanaAttackSFX);
    }

    public void playKatanaDeflectSFX()
    {
        katanaSFX.PlayOneShot(katanaDeflectSFX, katanaDeflectVolume);
    }

    public void playKatanaHitSFX()
    {
        katanaSFX.PlayOneShot(katanaHitSFX);
    }

    public void PlayDashSFX()
    {
        katanaSFX.PlayOneShot(dashSFX);
    }

    public void PlayGetHitSFX()
    {
        katanaSFX.PlayOneShot(getHitSFX);
    }

    public void PlayProjectileSlashSFX()
    {
        katanaSFX.PlayOneShot(projectileSlashSFX);
    }
}
