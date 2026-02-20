using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    [SerializeField] private AudioSource katanaSFX;

    private void Start()
    {
        katanaSFX = GameObject.FindGameObjectWithTag("Weapon").GetComponent<AudioSource>();
    }

    public void playKatanaSFX()
    {
        katanaSFX.Play();
    }
}
