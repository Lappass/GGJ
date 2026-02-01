using UnityEngine;
using System.Collections.Generic;

public class PickupItem : Interactable
{
    [Header("Settings")]
    [Tooltip("Unique ID for saving state across scenes")]
    [SerializeField] private string objectID = "Pickup_01";

    [Header("Priority")]
    [SerializeField] private int priority = 100;
    public override int Priority => priority;

    [Header("Pickup")]
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Reward")]
    [Tooltip("If set, unlocking this reward on pickup. If dialogue is shown, consider delaying via FragmentRewarder instead.")]
    [SerializeField] private System.Collections.Generic.List<GameObject> fragmentRewards;
    [SerializeField] private GameObject fragmentReward;
    
    [Header("Dialogue On Pickup")]
    [SerializeField] private bool showDialogue = true;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence dialogueSequence;

    [Header("Glow Hint")]
    [SerializeField] private bool enableGlowHint = false;
    [SerializeField] private GameObject glowVisual;
    [Range(0f, 1f)]
    [SerializeField] private float glowIntensity = 1f;
    [SerializeField] private bool checkIdentity = false;
    [SerializeField] private IdentityType requiredIdentity;
    [SerializeField] private List<EmotionType> requiredEmotions;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPickedUp;

    private bool _picked = false;
    private bool _prerequisitesMet = true;
    public override bool CanInteract => !_picked && _prerequisitesMet;

    private void Start()
    {
        // Try to auto-find dialogue manager if not assigned
        if (showDialogue && dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        if (GameStateManager.Instance != null)
        {
            _picked = GameStateManager.Instance.GetState(objectID);
            if (_picked)
            {
                if (destroyOnPickup) Destroy(gameObject);
                else gameObject.SetActive(false);
                return;
            }
        }

        // Always check conditions on start
        if (PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnMaskStateChanged += CheckConditions;
            CheckConditions();
        }
        
        // Initial state for glow
        if (enableGlowHint && glowVisual != null) glowVisual.SetActive(false);
    }

    private void OnDestroy()
    {
        if (PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnMaskStateChanged -= CheckConditions;
        }
    }

    private void CheckConditions()
    {
        if (PlayerMaskInventoryController.Instance == null) return;

        // Check Identity
        bool identityMatch = true;
        if (checkIdentity)
        {
            identityMatch = PlayerMaskInventoryController.Instance.CurrentIdentity == requiredIdentity;
        }

        // Check Emotions
        bool emotionMatch = true;
        if (requiredEmotions != null && requiredEmotions.Count > 0)
        {
            // Verify the current mask has all required emotions
            foreach (var reqEmo in requiredEmotions)
            {
                if (!PlayerMaskInventoryController.Instance.CurrentEmotions.Contains(reqEmo))
                {
                    emotionMatch = false;
                    break;
                }
            }
        }

        _prerequisitesMet = identityMatch && emotionMatch;

        // Update Glow Visual
        if (enableGlowHint && glowVisual != null)
        {
            bool shouldGlow = _prerequisitesMet;
            glowVisual.SetActive(shouldGlow);

            if (shouldGlow)
            {
                var sr = glowVisual.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = glowIntensity;
                    sr.color = c;
                }
            }
        }
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (_picked) return;
        if (!_prerequisitesMet) return; // Enforce conditions check

        _picked = true;

        // Reward logic
        if (PlayerMaskInventoryController.Instance != null)
        {
             if (fragmentReward != null)
                 PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
                 
             if (fragmentRewards != null)
             {
                 foreach(var f in fragmentRewards)
                 {
                     if (f != null) PlayerMaskInventoryController.Instance.UnlockFragment(f);
                 }
             }
        }

        // Save state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }

        onPickedUp?.Invoke();

        if (showDialogue && dialogueManager != null && dialogueSequence != null
            && dialogueSequence.lines != null && dialogueSequence.lines.Count > 0)
        {
            dialogueManager.Play(dialogueSequence);
        }

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}