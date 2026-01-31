using UnityEngine;

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
    [SerializeField] private GameObject fragmentReward;
    
    [Header("Dialogue On Pickup")]
    [SerializeField] private bool showDialogue = true;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence dialogueSequence;

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
            }
        }
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (_picked) return;
        _picked = true;

        // Reward logic
        if (fragmentReward != null && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
        }

        // Save state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }

        if (showDialogue && dialogueManager != null && dialogueSequence != null
            && dialogueSequence.lines != null && dialogueSequence.lines.Count > 0)
        {
            dialogueManager.Play(dialogueSequence);
        }

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}