using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource katanaSFX;
    [SerializeField] private AudioSource playerSFX;
    [SerializeField] private AudioSource koSFX;

    [SerializeField] private AudioClip katanaAttackSFX;
    [SerializeField] private AudioClip katanaDeflectSFX;
    [SerializeField] private AudioClip katanaHitSFX;
    [SerializeField] private AudioClip dashSFX;
    [SerializeField] private AudioClip reviveSFX;

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
        playerSFX.PlayOneShot(dashSFX);
    }

    public void PlayReviveSFX()
    {

    }
}
