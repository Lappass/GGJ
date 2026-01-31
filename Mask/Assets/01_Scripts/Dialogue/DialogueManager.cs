using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private CanvasGroup rootCanvasGroup;   // DialogueRoot 上的 CanvasGroup
    [SerializeField] private TMP_Text speakerText;          // NameText (TMP)
    [SerializeField] private TMP_Text contentText;          // BodyText (TMP)
    [SerializeField] private GameObject continueIndicator;  // Triangle

    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private int mouseButton = 0;           // 0 = Left Mouse

    [Header("Behavior")]
    [Tooltip("每句出现后，多少秒后自动显示小三角")]
    [SerializeField] private float autoShowIndicatorAfterSeconds = 5f;
    [Tooltip("是否使用 Unscaled Time（暂停/慢动作时仍正常计时）")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Events (Optional)")]
    public UnityEvent onDialogueStarted;
    public UnityEvent onDialogueFinished;

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

        // 计时：用于 5 秒后自动显示三角
        if (_state == LineState.WaitingToRevealIndicator)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt;

            if (_timer >= autoShowIndicatorAfterSeconds)
                RevealIndicator();
        }

        // 输入：左键或空格
        if (Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(advanceKey))
            HandleAdvanceInput();
    }

    private void HandleAdvanceInput()
    {
        // 前 5 秒内：第一次点击只唤出三角，不推进
        if (_state == LineState.WaitingToRevealIndicator)
        {
            RevealIndicator();
            return;
        }

        // 三角出现后：点击推进下一句
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

        // 新一句出现：隐藏三角，重新计时，回到“等待唤出三角”状态
        if (continueIndicator != null) continueIndicator.SetActive(false);
        _timer = 0f;
        _state = LineState.WaitingToRevealIndicator;
    }

    // 只保留这个 Play：不再支持“允许/不允许跳过”的开关
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
            // 兜底：没有 CanvasGroup 时用 SetActive 控制（不推荐，但可用）
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
