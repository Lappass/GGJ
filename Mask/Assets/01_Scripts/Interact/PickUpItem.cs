using UnityEngine;

public class PickupItem : Interactable
{
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

    public override void Interact(PlayerInteractor interactor)
    {
        if (_picked) return;
        _picked = true;

        // Reward logic
        if (fragmentReward != null && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
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