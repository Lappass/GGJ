using UnityEngine;
using System.Collections.Generic;

public class MaskStageDialogueAutoRunner : MonoBehaviour
{
    [Header("Refs")]
    public DialogueManager dialogueManager;
    public MaskStageDialogueAsset dialogueAsset;

    [Header("Progress Key")]
    [Tooltip("用于保存/读取Stage进度的Key。不同场景/不同NPC要用不同Key")]
    public string progressKey = "MaskStage_Default";

    [Header("Options")]
    [Tooltip("进入场景后自动播放一次")]
    public bool playOnStart = true;

    [Tooltip("如果对话正在播放，是否等待播放结束后再触发本次自动播放")]
    public bool waitIfDialoguePlaying = true;

    [Tooltip("到最后一关后是否停在最后一关（true=重复最后一关；false=超过后不播）")]
    public bool clampAtLastStage = true;

    [Tooltip("延迟多少秒后触发（给场景淡入/玩家状态初始化留时间）")]
    public float startDelay = 0.1f;

    [Header("Optional Fallback")]
    public DialogueSequence fallbackDialogue;

    private bool _hasAutoPlayedThisScene = false;

    private void Start()
    {
        if (!playOnStart) return;
        Invoke(nameof(TryAutoPlay), Mathf.Max(0f, startDelay));
    }

    public void TryAutoPlay()
    {
        if (_hasAutoPlayedThisScene) return;
        _hasAutoPlayedThisScene = true;

        if (dialogueManager == null || dialogueAsset == null)
        {
            PlayFallback();
            return;
        }

        if (waitIfDialoguePlaying && dialogueManager.IsPlaying)
        {
            // 简单等一帧再试；不写协程也行
            Invoke(nameof(TryAutoPlay), 0.1f);
            _hasAutoPlayedThisScene = false; // 允许重试
            return;
        }

        if (dialogueAsset.stages == null || dialogueAsset.stages.Count == 0)
        {
            PlayFallback();
            return;
        }

        int stageIndex = LoadStage();

        if (!clampAtLastStage && stageIndex >= dialogueAsset.stages.Count)
        {
            // 超过最后一关且不clamp：不播
            return;
        }

        // clamp
        int useIndex = stageIndex;
        if (clampAtLastStage)
        {
            if (useIndex < 0) useIndex = 0;
            if (useIndex >= dialogueAsset.stages.Count) useIndex = dialogueAsset.stages.Count - 1;
        }

        // 玩家面具状态（从 MaskManager 读取）
        IdentityType playerIdentity = (IdentityType)0;
        IList<EmotionType> playerEmotions = null;

        if (MaskManager.Instance != null)
        {
            playerIdentity = MaskManager.Instance.CurrentIdentity;
            playerEmotions = MaskManager.Instance.CurrentEmotions;
        }

        bool correct = dialogueAsset.IsCorrect(useIndex, playerIdentity, playerEmotions);
        var stage = dialogueAsset.GetStageClamped(useIndex);

        if (stage == null)
        {
            PlayFallback();
            return;
        }

        if (correct)
        {
            if (IsValid(stage.correctDialogue))
            {
                dialogueManager.Play(stage.correctDialogue, () =>
                {
                    SaveStage(stageIndex + 1);
                });
            }
            else
            {
                // 没填正确对话也推进
                SaveStage(stageIndex + 1);
            }
        }
        else
        {
            // 错误：不推进，反复留在当前stage
            if (IsValid(stage.wrongDialogue))
                dialogueManager.Play(stage.wrongDialogue);
            else
                PlayFallback();
        }
    }

    private void PlayFallback()
    {
        if (dialogueManager == null) return;
        if (IsValid(fallbackDialogue))
            dialogueManager.Play(fallbackDialogue);
    }

    private bool IsValid(DialogueSequence seq)
    {
        return seq != null && seq.lines != null && seq.lines.Count > 0;
    }

    private int LoadStage()
    {
        return PlayerPrefs.GetInt(progressKey, 0);
    }

    private void SaveStage(int newIndex)
    {
        PlayerPrefs.SetInt(progressKey, newIndex);
        PlayerPrefs.Save();
    }
}
