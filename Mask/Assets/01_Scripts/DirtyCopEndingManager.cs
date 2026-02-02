using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DirtyCopEndingManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueManager dialogueManager;
    
    [Header("Configuration")]
    [Tooltip("The identity required to trigger this sequence.")]
    [SerializeField] private IdentityType targetIdentity = IdentityType.DirtyCop;
    [Tooltip("If true, checks condition automatically on Start.")]
    [SerializeField] private bool checkOnStart = false;
    [Tooltip("If true, automatically listens for mask changes and triggers if condition is met.")]
    [SerializeField] private bool autoCheckOnMaskChange = true;

    [Header("UI References")]
    [Tooltip("The panel that will act as the black screen (should block raycasts).")]
    [SerializeField] private GameObject blackScreenPanel;
    [Tooltip("The UI element to show in the center of the screen.")]
    [SerializeField] private GameObject centerUIElement;
    [Tooltip("Duration for the black screen fade-in (if it has a CanvasGroup).")]
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Dialogue")]
    [SerializeField] private DialogueSequence dialogueSequence;

    private bool _hasTriggered = false;

    private void Start()
    {
        // Try to find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        // Ensure UI is hidden initially
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
        if (centerUIElement != null) centerUIElement.SetActive(false);

        if (checkOnStart)
        {
            CheckAndTrigger();
        }

        if (autoCheckOnMaskChange && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnMaskStateChanged += CheckAndTrigger;
        }
    }

    private void OnDestroy()
    {
        if (autoCheckOnMaskChange && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnMaskStateChanged -= CheckAndTrigger;
        }
    }

    /// <summary>
    /// Checks if the player has the DirtyCop identity and triggers the sequence if so.
    /// </summary>
    public void CheckAndTrigger()
    {
        if (_hasTriggered) return;

        if (PlayerMaskInventoryController.Instance == null)
        {
            Debug.LogWarning("[DirtyCopEndingManager] PlayerMaskInventoryController is missing!");
            return;
        }

        if (PlayerMaskInventoryController.Instance.CurrentIdentity == targetIdentity)
        {
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        _hasTriggered = true;
        Debug.Log("[DirtyCopEndingManager] Triggering Ending Sequence.");

        // 1. Show Black Screen
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            CanvasGroup cg = blackScreenPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                    yield return null;
                }
                cg.alpha = 1f;
            }
        }

        // 2. Show Center UI
        if (centerUIElement != null)
        {
            centerUIElement.SetActive(true);
        }

        // Wait a small moment before dialogue?
        yield return new WaitForSeconds(0.5f);

        // 3. Trigger Dialogue
        if (dialogueSequence != null && dialogueSequence.lines != null && dialogueSequence.lines.Count > 0)
        {
            if (dialogueManager != null)
            {
                dialogueManager.Play(dialogueSequence, OnSequenceComplete);
            }
            else
            {
                Debug.LogError("[DirtyCopEndingManager] DialogueManager reference is missing!");
            }
        }
        else
        {
            Debug.LogWarning("[DirtyCopEndingManager] No dialogue sequence assigned.");
            OnSequenceComplete();
        }
    }

    private void OnSequenceComplete()
    {
        Debug.Log("[DirtyCopEndingManager] Sequence Complete.");
        // Add any logic here that happens after dialogue ends
    }
}
