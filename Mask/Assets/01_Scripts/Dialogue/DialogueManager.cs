using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup rootCanvasGroup;   // DialogueRoot �ϵ� CanvasGroup
    [SerializeField] private TMP_Text speakerText;          // NameText (TMP)
    [SerializeField] private TMP_Text contentText;          // BodyText (TMP)
    [SerializeField] private GameObject continueIndicator;  // Triangle

    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private int mouseButton = 0;           // 0 = Left Mouse

    [Header("Behavior")]
    [SerializeField] private float autoShowIndicatorAfterSeconds = 5f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("SFX")]
    [SerializeField] private AudioSource uiSfxSource;
    [SerializeField] private AudioClip advanceSfx;
    [SerializeField] private bool playOnOpen = false;
    [SerializeField] private bool playOnAdvance = true;

    [Header("Events (Optional)")]
    public UnityEvent onDialogueStarted;
    public UnityEvent onDialogueFinished;

    // Static events for code-based subscribers
    public static event System.Action OnGlobalDialogueStart;
    public static event System.Action OnGlobalDialogueEnd;

    private System.Action _onCompleteCallback;

    public bool IsPlaying => _active;

    private DialogueSequence _sequence;
    private int _index = -1;

    private float _timer = 0f;
    private bool _active = false;

    private enum LineState { WaitingToRevealIndicator, ReadyToAdvance }
    private LineState _state = LineState.WaitingToRevealIndicator;

    private void Awake()
    {
        if (uiSfxSource == null) uiSfxSource = GetComponent<AudioSource>();
        HideAllImmediate();

    }
    private void PlayAdvanceSfx()
    {
        if (uiSfxSource != null && advanceSfx != null)
            uiSfxSource.PlayOneShot(advanceSfx);
    }

    private void AdvanceDialogue()
    {
        if (playOnAdvance) PlayAdvanceSfx();
    }

    private void Update()
    {
        if (!_active) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }

        if (_state == LineState.WaitingToRevealIndicator)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;

            if (_timer >= autoShowIndicatorAfterSeconds)
                RevealIndicator();
        }

        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(advanceKey))
            HandleAdvanceInput();
    }

    private void HandleAdvanceInput()
    {
        if (_state == LineState.WaitingToRevealIndicator)
        {
            RevealIndicator();
            return;
        }

        if (_state == LineState.ReadyToAdvance)
            ShowNextLine();
    }

    private void RevealIndicator()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(true);

        _state = LineState.ReadyToAdvance;
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

    public void Play(DialogueSequence sequence, System.Action onComplete = null)
    {
        Debug.Log($"[DialogueManager] Play requested. Sequence: {sequence}");
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Empty sequence, stopping immediately.");
            onComplete?.Invoke();
            return;
        }
        if (playOnOpen) PlayAdvanceSfx();

        _onCompleteCallback = onComplete;
        _sequence = sequence;
        _index = -1;
        _active = true;

        Debug.Log("[DialogueManager] Setting root visible and starting.");
        SetRootVisible(true);

        onDialogueStarted?.Invoke();
        OnGlobalDialogueStart?.Invoke();
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
        OnGlobalDialogueEnd?.Invoke();
        _onCompleteCallback?.Invoke();
        _onCompleteCallback = null;
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
            // ���ף�û�� CanvasGroup ʱ�� SetActive ���ƣ����Ƽ��������ã�
            gameObject.SetActive(visible);
        }
    }

    private void HideAllImmediate()
    {
        SetRootVisible(false);

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        _timer = 0f;
        _state = LineState.WaitingToRevealIndicator;
    }
}
