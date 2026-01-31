using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;

        [TextArea(2, 6)]
        public string content;
    }

    [Serializable]
    public class DialogueSequence
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
    }

    [Header("UI Refs")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject continueIndicator; 

    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private int mouseButton = 0;

    [Header("Behavior")]
    [SerializeField] private float autoShowIndicatorAfterSeconds = 5f;
    [SerializeField] private bool useUnscaledTime = true;

    public UnityEvent onDialogueStarted;
    public UnityEvent onDialogueFinished;

    public bool IsPlaying => _active;

    private DialogueSequence _sequence;
    private int _index = -1;
    private float _timer = 0f;
    private bool _active = false;

    private enum LineState
    {
        WaitingToRevealIndicator,
        ReadyNext
    }

    private LineState _state = LineState.WaitingToRevealIndicator;

    private void Awake()
    {
        HideAllImmediate();
    }

    private void Update()
    {
        if (!_active) return;

        if (_state == LineState.WaitingToRevealIndicator)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;

            if (_timer >= autoShowIndicatorAfterSeconds)
            {
                RevealIndicator();
            }
        }

        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(advanceKey))
        {
            HandleAdvanceInput();
        }
    }

    private void HandleAdvanceInput()
    {
        if (_state == LineState.WaitingToRevealIndicator)
        {
            RevealIndicator();
            return;
        }
        if (_state == LineState.ReadyNext)
        {
            ShowNextLine();
        }
    }

    private void RevealIndicator()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(true);

        _state = LineState.ReadyNext;
    }

    private void ShowNextLine()
    {
        if (_sequence == null || _sequence.lines == null)
        {
            Stop();
            return;
        }

        _index++;
        if (_index >= _sequence.lines.Count)
        {
            Stop();
            return;
        }

        var line = _sequence.lines[_index];

        if (speakerText != null) speakerText.text = line.speaker;
        if (contentText != null) contentText.text = line.content;

        if (continueIndicator != null) continueIndicator.SetActive(false);
        _timer = 0f;
        _state = LineState.WaitingToRevealIndicator;
    }

    public void Play(DialogueSequence sequence)
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
            return;

        _sequence = sequence;
        _index = -1;
        _active = true;

        SetRootVisible(true);

        onDialogueStarted?.Invoke();
        ShowNextLine();
    }

    public void Stop()
    {
        if (!_active) return;

        _active = false;
        _sequence = null;
        _index = -1;

        HideAllImmediate();
        onDialogueFinished?.Invoke();
    }

    private void SetRootVisible(bool visible)
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = visible ? 1f : 0f;
            rootCanvasGroup.blocksRaycasts = visible;
            rootCanvasGroup.interactable = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    private void HideAllImmediate()
    {
        SetRootVisible(false);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }
}
