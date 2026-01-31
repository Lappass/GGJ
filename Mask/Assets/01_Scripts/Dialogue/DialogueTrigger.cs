using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueManager dialogueUI;
    public DialogueManager.DialogueSequence sequence;

    public bool requireInteractKey = true;
    public KeyCode interactKey = KeyCode.E;
    public bool playOnce = true;

    private bool _hasPlayed = false;
    private bool _playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;

        if (!requireInteractKey)
            TryPlay();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
    }

    private void Update()
    {
        if (!_playerInside) return;
        if (!requireInteractKey) return;

        if (Input.GetKeyDown(interactKey))
            TryPlay();
    }

    private void TryPlay()
    {
        if (dialogueUI == null) return;
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0) return;

        if (dialogueUI.IsPlaying) return;
        if (playOnce && _hasPlayed) return;

        _hasPlayed = true;
        dialogueUI.Play(sequence);
    }
}
