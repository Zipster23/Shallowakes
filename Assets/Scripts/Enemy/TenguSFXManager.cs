using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenguSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource katanaSFX;
    [SerializeField] private AudioClip katanaAttackSFX;
    [SerializeField] private AudioClip katanaDeflectSFX;
    [SerializeField] private AudioClip katanaHitSFX;
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
}
