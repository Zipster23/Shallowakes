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
    [SerializeField] private GameObject parryDummy;          // Drag Dummy1 here
    [SerializeField] private GameObject attackDummy;         // Drag Dummy2 here

    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerVFXManager playerVFXManager;

    [Header("Tutorial Dialogue")]
    [SerializeField] private string[] introLines;
    [SerializeField] private string[] parryInstructionLines;
    [SerializeField] private string[] attackInstructionLines;
    [SerializeField] private string[] completionLines;

    private int currentStep = 0;
    private int lineIndex = 0;
    private bool dialogueActive = false;
    private string[] currentLines;
    private Coroutine typingCoroutine;
    private void Start()
    {
        dialoguePanel.SetActive(false);
        icon.SetActive(false);

        // Make sure dummies are disabled
        if (parryDummy != null)
            parryDummy.SetActive(false);
        if (attackDummy != null)
            attackDummy.SetActive(false);

        // Kasa Obake shoulder setup
        if (kasaObakeOnShallo != null)
        {
            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = true;

            // Make sure shoulder Kasa Obake visual is hidden initially
            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OnPlayerMeetKasaObake()
    {
        StartTutorial();
    }

    private void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (dialogueText.text == currentLines[lineIndex])
            {
                NextLine();
            }
            else
            {
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
        currentStep = 0;
        ShowDialogueSequence(introLines);
    }

    private void ShowDialogueSequence(string[] lines)
    {
        currentLines = lines;
        lineIndex = 0;
        dialogueActive = true;
        dialoguePanel.SetActive(true);
        icon.SetActive(true);
        dialogueText.text = string.Empty;
        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in currentLines[lineIndex].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
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
        }
        else
        {
            dialogueActive = false;
            icon.SetActive(false);
            dialoguePanel.SetActive(false);
            dialogueText.text = string.Empty;
            StopAllCoroutines();

            ProceedToNextStep();
        }
    }

    private void ProceedToNextStep()
    {
        currentStep++;

        switch (currentStep)
        {
            case 1:
                // After intro, Kasa Obake jumps on shoulder then leaves
                StartCoroutine(KasaObakeTemporaryAppearance());
                break;

            case 2:
                // After parry success, show attack instructions
                StartCoroutine(DelayedAttackSetup());
                break;

            case 3:
                // After attack success, show completion
                StartCoroutine(DelayedCompletion());
                break;
        }
    }

    // Kasa Obake appears briefly then disappears (enables glide)
    IEnumerator KasaObakeTemporaryAppearance()
    {
        yield return new WaitForSeconds(0.5f);

        // VFX at world Kasa Obake position
        if (kasaObakeNPC != null && playerVFXManager != null)
        {
            playerVFXManager.PlayGetHitVFX(kasaObakeNPC.transform.position);
        }

        // Hide world Kasa Obake
        if (kasaObakeNPC != null && kasaObakeNPC.transform.childCount > 0)
        {
            kasaObakeNPC.transform.GetChild(0).gameObject.SetActive(false);
        }

        // Show shoulder Kasa Obake
        if (kasaObakeOnShallo != null)
        {
            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = false;  // Disable animator temporarily

            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(true);

            Debug.Log("Shoulder Kasa Obake enabled");
        }

        // Wait for the disappear delay
        yield return new WaitForSeconds(kasaObakeDisappearDelay);

        // VFX at shoulder position
        if (kasaObakeOnShallo != null && playerVFXManager != null)
        {
            playerVFXManager.PlayGetHitVFX(kasaObakeOnShallo.transform.position);
        }

        // CRITICAL: Hide shoulder Kasa Obake visual again
        if (kasaObakeOnShallo != null)
        {
            if (kasaObakeOnShallo.transform.childCount > 0)
                kasaObakeOnShallo.transform.GetChild(0).gameObject.SetActive(false);

            Animator koAnimator = kasaObakeOnShallo.GetComponent<Animator>();
            if (koAnimator != null)
                koAnimator.enabled = true;  // Re-enable animator for glide events

            Debug.Log("Shoulder Kasa Obake hidden - animator re-enabled");
        }

        // Set glide flag
        if (playerMovement != null)
        {
            playerMovement.metKasaObake = true;
            Debug.Log("metKasaObake = TRUE - Glide enabled!");
        }

        yield return new WaitForSeconds(1f);

        // Now start the parry training
        StartCoroutine(DelayedParrySetup());
    }

    IEnumerator DelayedParrySetup()
    {
        yield return new WaitForSeconds(0.5f);
        ShowDialogueSequence(parryInstructionLines);
        yield return new WaitUntil(() => !dialogueActive);
        yield return new WaitForSeconds(0.5f);
        SpawnParryDummy();
    }

    IEnumerator DelayedAttackSetup()
    {
        yield return new WaitForSeconds(1f);
        ShowDialogueSequence(attackInstructionLines);
        yield return new WaitUntil(() => !dialogueActive);
        yield return new WaitForSeconds(0.5f);
        SpawnAttackDummy();
    }

    IEnumerator DelayedCompletion()
    {
        yield return new WaitForSeconds(1f);
        ShowDialogueSequence(completionLines);
        yield return new WaitUntil(() => !dialogueActive);
        yield return new WaitForSeconds(2f);
        EndTutorial();
    }

    private void SpawnParryDummy()
    {
        Debug.Log("SpawnParryDummy called");

        if (parryDummy == null)
        {
            Debug.LogError("Parry dummy NULL");
            return;
        }

        Debug.Log("Before enable: " + parryDummy.activeSelf);

        parryDummy.SetActive(true);

        Debug.Log("After enable self: " + parryDummy.activeSelf);
        Debug.Log("After enable hierarchy: " + parryDummy.activeInHierarchy);

        TutorialDummy dummyScript = parryDummy.GetComponent<TutorialDummy>();

        Debug.Log("Dummy script: " + dummyScript);

        if (dummyScript != null)
        {
            dummyScript.Initialize(
                TutorialDummy.TutorialAction.Parry,
                OnParryCompleted
            );
        }
    }

    private void SpawnAttackDummy()
    {
        Debug.Log("Spawning attack dummy");

        if (attackDummy == null)
        {
            Debug.LogError("Attack dummy is NULL! Assign Dummy2 in inspector!");
            return;
        }

        attackDummy.SetActive(true);

        TutorialDummy dummyScript = attackDummy.GetComponent<TutorialDummy>();
        if (dummyScript != null)
        {
            dummyScript.Initialize(TutorialDummy.TutorialAction.Attack, OnAttackCompleted);
        }
        else
        {
            Debug.LogError("TutorialDummy script not found on attack dummy!");
        }
    }

    private void OnParryCompleted()
    {
        Debug.Log("Parry completed!");
        parryDummy.SetActive(false);
        ProceedToNextStep();
    }

    private void OnAttackCompleted()
    {
        Debug.Log("Attack completed!");
        attackDummy.SetActive(false);
        ProceedToNextStep();
    }

    private void EndTutorial()
    {
        Debug.Log("Tutorial complete!");
        // Load your next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("YourNextSceneName");
    }
}