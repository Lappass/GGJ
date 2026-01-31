using UnityEngine;

public class DialogueTrigger : Interactable
{
    [Header("Dialogue")]
    public DialogueManager dialogueUI;
    public DialogueSequence sequence;

    [Header("Trigger Settings")]
    public bool playOnce = true;

    [Header("Priority")]
    [SerializeField] private int priority = 50;
    public override int Priority => priority;

    private bool _hasPlayed = false;

    public override bool CanInteract
    {
        get
        {
            if (dialogueUI == null) return false;
            if (sequence == null || sequence.lines == null || sequence.lines.Count == 0) return false;
            if (dialogueUI.IsPlaying) return false;
            if (playOnce && _hasPlayed) return false;
            return true;
        }
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract) return;

        _hasPlayed = true;
        dialogueUI.Play(sequence);
    }
}
