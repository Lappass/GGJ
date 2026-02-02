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
    [Tooltip("ÿ����ֺ󣬶�������Զ���ʾС����")]
    [SerializeField] private float autoShowIndicatorAfterSeconds = 5f;
    [Tooltip("�Ƿ�ʹ�� Unscaled Time����ͣ/������ʱ��������ʱ��")]
    [SerializeField] private bool useUnscaledTime = true;

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
        HideAllImmediate();
    }

    private void Update()
    {
        if (!_active) return;

        // ��ʱ������ 5 ����Զ���ʾ����
        if (_state == LineState.WaitingToRevealIndicator)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;

            if (_timer >= autoShowIndicatorAfterSeconds)
                RevealIndicator();
        }

        // ���룺�����ո�
        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(advanceKey))
            HandleAdvanceInput();
    }

    private void HandleAdvanceInput()
    {
        // ǰ 5 ���ڣ���һ�ε��ֻ�������ǣ����ƽ�
        if (_state == LineState.WaitingToRevealIndicator)
        {
            RevealIndicator();
            return;
        }

        // ���ǳ��ֺ󣺵���ƽ���һ��
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

        // ��һ����֣��������ǣ����¼�ʱ���ص����ȴ��������ǡ�״̬
        if (continueIndicator != null) continueIndicator.SetActive(false);
        _timer = 0f;
        _state = LineState.WaitingToRevealIndicator;
    }

    // ֻ������� Play������֧�֡�����/�������������Ŀ���
    public void Play(DialogueSequence sequence, System.Action onComplete = null)
    {
        Debug.Log($"[DialogueManager] Play requested. Sequence: {sequence}");
        if (sequence == null || sequence.lines == null || sequence.lines.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] Empty sequence, stopping immediately.");
            onComplete?.Invoke();
            return;
        }

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
