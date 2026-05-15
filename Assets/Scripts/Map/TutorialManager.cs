using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject icon;
    [SerializeField] private float textSpeed = 0.05f;

    [Header("Kasa Obake NPC")]
    [SerializeField] private GameObject kasaObakeNPC;
    [SerializeField] private GameObject kasaObakeOnShallo;
    [SerializeField] private float kasaObakeDisappearDelay = 3f;

    [Header("Tutorial Dummies")]
    [SerializeField] private GameObject parryDummy;
    [SerializeField] private GameObject firstDummy;
    [SerializeField] private GameObject dodgeDummy;

    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerVFXManager playerVFXManager;

    [Header("Tutorial Dialogue")]
    [SerializeField] private string[] introLines;
    [SerializeField] private string[] parryInstructionLines;
    [SerializeField] private string[] DodgeInstructionLines;
    [SerializeField] private string[] completionLines;

    private int currentStep = 0;
    private int lineIndex = 0;
    private bool dialogueActive = false;
    private string[] currentLines;
    private Coroutine typingCoroutine;
    [SerializeField] GameObject playerPrefab;

    private void Start()
    {
        Debug.Log("TutorialManager Start() called");

        dialoguePanel.SetActive(false);
        icon.SetActive(false);

        if (parryDummy != null)
        {
            parryDummy.SetActive(false);
            Debug.Log("Parry dummy found and disabled");
        }
        else
        {
            Debug.LogError("PARRY DUMMY IS NULL! Assign it in inspector!");
        }

        if (dodgeDummy != null)
        {
            dodgeDummy.SetActive(true);
            Debug.Log("dodge dummy found and enabled");
        }
        else
        {
            Debug.LogError("DODGE DUMMY IS NULL! Assign it in inspector!");
        }

        if (kasaObakeOnShallo != null)
        {
            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = true;

            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OnPlayerMeetKasaObake()
    {
        Debug.Log("OnPlayerMeetKasaObake() called - starting tutorial");
        StartTutorial();
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed during dialogue");

            if (dialogueText.text == currentLines[lineIndex])
            {
                Debug.Log("Line complete, moving to next");
                NextLine();
            }
            else
            {
                Debug.Log("Skipping typing animation");
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueText.text = currentLines[lineIndex];
            }
        }
    }

    private void StartTutorial()
    {
        Debug.Log("StartTutorial() called");
        currentStep = 0;

        if (introLines == null || introLines.Length == 0)
        {
            Debug.LogError("Intro lines are empty! Add dialogue in inspector!");
            return;
        }

        ShowDialogueSequence(introLines);
    }

    private void ShowDialogueSequence(string[] lines)
    {
        Debug.Log($"ShowDialogueSequence called with {lines.Length} lines");

        currentLines = lines;
        lineIndex = 0;
        dialogueActive = true;
        dialoguePanel.SetActive(true);
        icon.SetActive(true);
        dialogueText.text = string.Empty;
        typingCoroutine = StartCoroutine(TypeLine()); ;
    }

    IEnumerator TypeLine()
    {
        Debug.Log($"Typing line {lineIndex}: {currentLines[lineIndex]}");

        foreach (char c in currentLines[lineIndex].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        Debug.Log("Line typing complete");
    }

    private void NextLine()
    {
        if (lineIndex < currentLines.Length - 1)
        {
            lineIndex++;
            dialogueText.text = string.Empty;
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeLine());
            Debug.Log($"Moving to line {lineIndex}");
        }
        else
        {
            Debug.Log("Dialogue sequence complete!");
            dialogueActive = false;
            icon.SetActive(false);
            dialoguePanel.SetActive(false);
            dialogueText.text = string.Empty;

            ProceedToNextStep();
        }
    }

    private void ProceedToNextStep()
    {
        currentStep++;
        Debug.Log($"ProceedToNextStep called - currentStep is now {currentStep}");

        switch (currentStep)
        {
            case 1:
                Debug.Log("Starting Kasa Obake appearance");
                StartCoroutine(KasaObakeTemporaryAppearance());
                break;

            case 2:
                Debug.Log("Starting attack setup");
                StartCoroutine(DelayedDodgeSetup());
                break;

            case 3:
                Debug.Log("Starting completion");
                StartCoroutine(DelayedCompletion());
                break;

            default:
                Debug.LogWarning($"Unexpected step: {currentStep}");
                break;
        }
    }

    IEnumerator KasaObakeTemporaryAppearance()
    {
        Debug.Log("Kasa Obake appearance coroutine started");
        yield return new WaitForSeconds(0.5f);

        if (kasaObakeNPC != null && playerVFXManager != null)
        {
            playerVFXManager.PlayGetHitVFX(kasaObakeNPC.transform.position);
            Debug.Log("Played VFX at Kasa Obake NPC");
        }

        if (kasaObakeNPC != null && kasaObakeNPC.transform.childCount > 0)
        {
            kasaObakeNPC.transform.GetChild(0).gameObject.SetActive(false);
            Debug.Log("Hid world Kasa Obake");
        }

        if (kasaObakeOnShallo != null)
        {
            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = false;

            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(true);

            Debug.Log("Shoulder Kasa Obake enabled");
        }

        yield return new WaitForSeconds(kasaObakeDisappearDelay);
        Debug.Log("Disappear delay finished");

        if (kasaObakeOnShallo != null && playerVFXManager != null)
        {
            playerVFXManager.PlayGetHitVFX(kasaObakeOnShallo.transform.position);
            Debug.Log("Played VFX at shoulder");
        }

        if (kasaObakeOnShallo != null)
        {
            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(false);

            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = true;

            Debug.Log("Shoulder Kasa Obake hidden - animator re-enabled");
        }

        if (playerMovement != null)
        {

        }

        yield return new WaitForSeconds(1f);

        Debug.Log("Starting parry setup after Kasa Obake sequence");
        StartCoroutine(DelayedParrySetup());
    }

    IEnumerator DelayedParrySetup()
    {
        Debug.Log("DelayedParrySetup coroutine started");
        yield return new WaitForSeconds(0.5f);

        if (parryInstructionLines == null || parryInstructionLines.Length == 0)
        {
            Debug.LogError("Parry instruction lines are empty!");
            yield break;
        }

        ShowDialogueSequence(parryInstructionLines);
        yield return new WaitUntil(() => !dialogueActive);
        Debug.Log("Parry instructions finished, spawning dummy in 0.5s");
        yield return new WaitForSeconds(0.5f);
        SpawnParryDummy();
    }

    IEnumerator DelayedDodgeSetup()
    {
        Debug.Log("DelayedAttackSetup coroutine started");
        yield return new WaitForSeconds(1f);

        if (DodgeInstructionLines == null || DodgeInstructionLines.Length == 0)
        {
            Debug.LogError("Dodge instruction lines are empty!");
            yield break;
        }

        playerMovement.metKasaObake = true;
        playerMovement.maxJumps = 2;
        Debug.Log("metKasaObake = TRUE - Glide enabled!");
        ShowDialogueSequence(DodgeInstructionLines);
        yield return new WaitUntil(() => !dialogueActive);
        Debug.Log("Dodge instructions finished, spawning dummy in 0.5s");
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator DelayedCompletion()
    {
        Debug.Log("DelayedCompletion coroutine started");
        yield return new WaitForSeconds(1f);

        if (completionLines == null || completionLines.Length == 0)
        {
            Debug.LogError("Completion lines are empty!");
            yield break;
        }

        ShowDialogueSequence(completionLines);
        yield return new WaitUntil(() => !dialogueActive);
        yield return new WaitForSeconds(2f);
    }

    private void SpawnParryDummy()
    {
        Debug.Log("SpawnParryDummy() called");

        if (parryDummy == null)
        {
            Debug.LogError("Parry dummy is NULL! Assign Dummy1 in inspector!");
            return;
        }

        Debug.Log($"Enabling parry dummy: {parryDummy.name}");
        parryDummy.SetActive(true);

        TutorialDummy dummyScript = parryDummy.GetComponent<TutorialDummy>();
        if (dummyScript != null)
        {
            Debug.Log("Initializing parry dummy script");
            dummyScript.Initialize(TutorialDummy.TutorialAction.Parry, OnParryCompleted);
        }
        else
        {
            Debug.LogError("TutorialDummy script not found on parry dummy!");
        }
    }

    private void OnParryCompleted()
    {
        Debug.Log("OnParryCompleted() callback triggered!");
        parryDummy.SetActive(false);
        ProceedToNextStep();
    }
}