using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource katanaSFX;

    public void playKatanaSFX()
    {
        katanaSFX.Play();
    }
}
