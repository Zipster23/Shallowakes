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
    [SerializeField] private GameObject kasaObakeOnShallo;
    [SerializeField] private GameObject lightBeam;

    public GameObject lightBeamTwo;

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
        lightBeamTwo.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
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
            StartCoroutine(TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
            icon.SetActive(false);
            bubble.SetActive(false);
            textComponent.text= string.Empty;
            kasaObakeOnShallo.SetActive(true);
            lightBeamTwo.SetActive(true);
        }
    }
}