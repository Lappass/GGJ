using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Layer Filter")]
    [SerializeField] private LayerMask interactableLayers;

    [Header("Prompt UI")]
    [SerializeField] private GameObject promptRoot;

    [Header("�Ի�ʱ���ý���!!")]
    [SerializeField] private DialogueManager dialogueManager;

    private readonly List<Interactable> _inRange = new List<Interactable>();
    private Interactable _current;
    private bool _canInteract = true;

    private void Awake()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    private void OnEnable()
    {
        DialogueManager.OnGlobalDialogueStart += OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd += OnDialogueEnd;
    }

    private void OnDisable()
    {
        DialogueManager.OnGlobalDialogueStart -= OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd -= OnDialogueEnd;
    }

    private void OnDialogueStart() => _canInteract = false;
    private void OnDialogueEnd() => _canInteract = true;

    private void Update()
    {
        // Check both the manual reference and the event flag
        bool dialoguePlaying = !_canInteract || (dialogueManager != null && dialogueManager.IsPlaying);

        if (dialoguePlaying)
        {
            if (promptRoot != null) promptRoot.SetActive(false);
            return;
        }

        SelectBest();
        if (promptRoot != null) promptRoot.SetActive(_current != null);

        if (_current != null && Input.GetKeyDown(interactKey))
        {
            _current.Interact(this);

            SelectBest();
            if (promptRoot != null) promptRoot.SetActive(_current != null);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger) return;
        if (((1 << other.gameObject.layer) & interactableLayers.value) == 0) return;

        var interactable = other.GetComponentInParent<Interactable>();
        if (interactable != null && !_inRange.Contains(interactable))
            _inRange.Add(interactable);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.isTrigger) return;
        if (((1 << other.gameObject.layer) & interactableLayers.value) == 0) return;

        var interactable = other.GetComponentInParent<Interactable>();
        if (interactable != null)
            _inRange.Remove(interactable);
    }

    private void SelectBest()
    {
        _current = null;

        int bestPriority = int.MinValue;
        float bestDist = float.MaxValue;

        for (int i = _inRange.Count - 1; i >= 0; i--)
        {
            var it = _inRange[i];
            if (it == null) { _inRange.RemoveAt(i); continue; }
            if (!it.CanInteract) continue;

            int p = it.Priority;
            float d = Vector2.Distance(transform.position, it.transform.position);

            if (p > bestPriority || (p == bestPriority && d < bestDist))
            {
                bestPriority = p;
                bestDist = d;
                _current = it;
            }
        }
    }
}
