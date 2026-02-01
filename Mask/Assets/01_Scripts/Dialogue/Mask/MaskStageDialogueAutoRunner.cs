using UnityEngine;
using System.Collections.Generic;

public class MaskStageDialogueAutoRunner : MonoBehaviour
{
    [Header("Refs")]
    public DialogueManager dialogueManager;
    public MaskStageDialogueAsset dialogueAsset;

    [Header("Progress Key")]
    public string progressKey = "InterrogationRoom_Main";

    [Header("Options")]
    public bool clampAtLastStage = true;
    public float startDelay = 0.2f;
    public bool ignoreIfDialoguePlaying = true;

    [Header("Optional Fallback")]
    public DialogueSequence fallbackDialogue;

    private void Start()
    {
        Invoke(nameof(EvaluateAndPlay), Mathf.Max(0f, startDelay));
    }

    public void EvaluateAndPlay()
    {
        if (dialogueManager == null || dialogueAsset == null)
        {
            PlayFallback();
            return;
        }

        if (ignoreIfDialoguePlaying && dialogueManager.IsPlaying)
            return;

        if (dialogueAsset.stages == null || dialogueAsset.stages.Count == 0)
        {
            PlayFallback();
            return;
        }

        int stageIndex = MaskStageProgressRuntime.GetStage(progressKey);

        if (!clampAtLastStage && stageIndex >= dialogueAsset.stages.Count)
            return;

        int useIndex = stageIndex;
        if (clampAtLastStage)
        {
            if (useIndex < 0) useIndex = 0;
            if (useIndex >= dialogueAsset.stages.Count) useIndex = dialogueAsset.stages.Count - 1;
        }

        var stage = dialogueAsset.GetStageClamped(useIndex);
        if (stage == null)
        {
            PlayFallback();
            return;
        }

        // 读取面具状态
        IdentityType identity = (IdentityType)0;
        IList<EmotionType> emotions = null;

        if (MaskManager.Instance != null)
        {
            identity = MaskManager.Instance.CurrentIdentity;
            emotions = MaskManager.Instance.CurrentEmotions;
        }

        bool correct = dialogueAsset.IsCorrect(useIndex, identity, emotions);
        Debug.Log($"[MaskStage] key={progressKey}, stage={useIndex}, correct={correct}, identity={identity}, emotions={FormatEmotions(emotions)}");

        if (correct)
        {
            if (IsValid(stage.correctDialogue))
            {
                dialogueManager.Play(stage.correctDialogue, () =>
                {
                    GrantStageRewards(stage, useIndex);
                    MaskStageProgressRuntime.SetStage(progressKey, stageIndex + 1);
                });
            }
            else
            {
                GrantStageRewards(stage, useIndex);
                MaskStageProgressRuntime.SetStage(progressKey, stageIndex + 1);
            }
        }
        else
        {
            if (IsValid(stage.wrongDialogue))
                dialogueManager.Play(stage.wrongDialogue);
            else
                PlayFallback();
        }
    }

    private void GrantStageRewards(MaskStage stage, int stageIndex)
    {
        if (stage == null) return;
        if (stage.fragmentsToGrant == null || stage.fragmentsToGrant.Count == 0) return;

        if (PlayerMaskInventoryController.Instance == null)
        {
            Debug.LogWarning("PlayerMaskInventoryController missing! Cannot grant fragments.");
            return;
        }

        if (stage.grantOnce && MaskStageProgressRuntime.HasGrantedReward(progressKey, stageIndex))
            return;

        int count = 0;
        foreach (var prefab in stage.fragmentsToGrant)
        {
            if (prefab == null) continue;
            PlayerMaskInventoryController.Instance.UnlockFragment(prefab);
            count++;
        }

        Debug.Log($"[StageReward] key={progressKey}, stage={stageIndex}, granted={count}");

        if (stage.grantOnce)
            MaskStageProgressRuntime.MarkRewardGranted(progressKey, stageIndex);
    }

    private void PlayFallback()
    {
        if (dialogueManager == null) return;
        if (IsValid(fallbackDialogue))
            dialogueManager.Play(fallbackDialogue);
    }

    private bool IsValid(DialogueSequence seq) => seq != null && seq.lines != null && seq.lines.Count > 0;

    private string FormatEmotions(IList<EmotionType> list)
    {
        if (list == null || list.Count == 0) return "(none)";
        return string.Join(",", list);
    }
}
