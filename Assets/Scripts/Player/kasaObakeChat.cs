using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class kasaObakeChat : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private string[] lines;
    [SerializeField] private float textSpeed;
    [SerializeField] private GameObject icon;
    [SerializeField] private GameObject bubble;
    [SerializeField] private GameObject lightBeam;
    [SerializeField] private GameObject kasaObakeOnShallo;
    [SerializeField] private float kasaObakeDisappearDelay;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] PlayerVFXManager playerVFXManager;

    private bool dialogueActive = false;
    private int index;
    private Collider koColider;
    // Start is called before the first frame update
    void Start()
    {
        koColider = GetComponent<Collider>();
        koColider.isTrigger = false;
        textComponent.text = string.Empty;
        icon.SetActive(false);
        bubble.SetActive(false);
        lightBeam.SetActive(true);

    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    void OnCollisionEnter()
    {
        koColider.isTrigger = true;
        icon.SetActive(true);
        bubble.SetActive(true);
        lightBeam.SetActive(false);
        StartDialogue();
    }

    void StartDialogue()
    {
        dialogueActive = true;
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())// takes string and break it into an array
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogueActive = false;    // stop listening before coroutine runs
            icon.SetActive(false);
            bubble.SetActive(false);
            textComponent.text = string.Empty;
            StopAllCoroutines();
            StartCoroutine(EnableKasaObake());
        }
    }

    IEnumerator EnableKasaObake()
    {
        Debug.Log("Coroutine started");
        playerVFXManager.PlayGetHitVFX(transform.position);
        transform.GetChild(0).gameObject.SetActive(false);

        Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
        koAnimator.enabled = false;
        kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(true);

        Debug.Log("Shoulder KO enabled");
        yield return new WaitForSeconds(kasaObakeDisappearDelay);
        Debug.Log("Delay finished");

        playerVFXManager.PlayGetHitVFX(kasaObakeOnShallo.transform.position);
        kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(false);
        koAnimator.enabled = true;
        playerMovement.metKasaObake = true;
    }
}