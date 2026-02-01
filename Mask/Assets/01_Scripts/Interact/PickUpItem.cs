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
    public override bool CanInteract => !_picked;

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

        if (enableGlowHint)
        {
            // Initial state
            if (glowVisual != null) glowVisual.SetActive(false);

            if (PlayerMaskInventoryController.Instance != null)
            {
                // Changed to listen for Inventory Updates (unlocks), not Mask State (equipped)
                PlayerMaskInventoryController.Instance.OnInventoryUpdated += CheckGlow;
                CheckGlow();
            }
        }
    }

    private void OnDestroy()
    {
        if (enableGlowHint && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnInventoryUpdated -= CheckGlow;
        }
    }

    private void CheckGlow()
    {
        if (!enableGlowHint || glowVisual == null) return;
        if (PlayerMaskInventoryController.Instance == null) return;

        var unlockedFragments = PlayerMaskInventoryController.Instance.GetUnlockedFragments();
        
        bool identityMatch = true;
        if (checkIdentity)
        {
            identityMatch = false; 
            // Check if any unlocked fragment provides this identity
            foreach (var fragment in unlockedFragments)
            {
                if (fragment == null) continue;
                var ui = fragment.GetComponent<DraggableUI>();
                if (ui != null && ui.attributeData != null)
                {
                    if (ui.attributeData.type == AttributeType.Identity && 
                        ui.attributeData.identityValue == requiredIdentity)
                    {
                        identityMatch = true;
                        break;
                    }
                }
            }
        }

        bool emotionMatch = true;
        if (requiredEmotions != null && requiredEmotions.Count > 0)
        {
            // Collect all unlocked emotion types
            HashSet<EmotionType> unlockedEmotions = new HashSet<EmotionType>();
            foreach (var fragment in unlockedFragments)
            {
                if (fragment == null) continue;
                var ui = fragment.GetComponent<DraggableUI>();
                if (ui != null && ui.attributeData != null && ui.attributeData.type == AttributeType.Emotion)
                {
                    unlockedEmotions.Add(ui.attributeData.emotionValue);
                }
            }

            // Verify we have all required emotions
            foreach (var reqEmo in requiredEmotions)
            {
                if (!unlockedEmotions.Contains(reqEmo))
                {
                    emotionMatch = false;
                    break;
                }
            }
        }

        bool shouldGlow = identityMatch && emotionMatch;
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

    public override void Interact(PlayerInteractor interactor)
    {
        if (_picked) return;
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