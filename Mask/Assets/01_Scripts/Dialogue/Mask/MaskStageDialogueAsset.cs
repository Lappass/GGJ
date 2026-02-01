using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MaskStage
{
    [Header("Correct Answer")]
    public IdentityType identity;

    [Tooltip("最多 3 个情绪槽；None 表示不填")]
    public EmotionType emotion1 = EmotionType.None;
    public EmotionType emotion2 = EmotionType.None;
    public EmotionType emotion3 = EmotionType.None;

    [Header("Dialogue - Correct (advance stage)")]
    public DialogueSequence correctDialogue;

    [Header("Dialogue - Wrong (stay in same stage)")]
    public DialogueSequence wrongDialogue;

    public void GetAnswerEmotionCounts(Dictionary<EmotionType, int> dict)
    {
        dict.Clear();
        AddEmotion(dict, emotion1);
        AddEmotion(dict, emotion2);
        AddEmotion(dict, emotion3);
    }

    private void AddEmotion(Dictionary<EmotionType, int> dict, EmotionType e)
    {
        if (e == EmotionType.None) return;
        if (!dict.ContainsKey(e)) dict[e] = 0;
        dict[e]++;
    }
}

[CreateAssetMenu(fileName = "NewMaskStageDialogue", menuName = "Dialogue/Mask Stage Dialogue")]
public class MaskStageDialogueAsset : ScriptableObject
{
    public List<MaskStage> stages = new List<MaskStage>();

    // 精确匹配：Identity 必须相等；情绪“多重集合”必须完全相等（忽略顺序；None 不算）
    public bool IsCorrect(int stageIndex, IdentityType playerIdentity, IList<EmotionType> playerEmotions)
    {
        if (stages == null || stages.Count == 0) return false;
        if (stageIndex < 0 || stageIndex >= stages.Count) return false;

        var stage = stages[stageIndex];
        if (stage == null) return false;

        if (!stage.identity.Equals(playerIdentity)) return false;

        // build counts
        var stageCounts = new Dictionary<EmotionType, int>();
        var playerCounts = new Dictionary<EmotionType, int>();

        stage.GetAnswerEmotionCounts(stageCounts);
        BuildPlayerEmotionCounts(playerEmotions, playerCounts);

        if (stageCounts.Count != playerCounts.Count) return false;

        foreach (var kv in stageCounts)
        {
            if (!playerCounts.TryGetValue(kv.Key, out int c)) return false;
            if (c != kv.Value) return false;
        }

        return true;
    }

    private void BuildPlayerEmotionCounts(IList<EmotionType> emotions, Dictionary<EmotionType, int> dict)
    {
        dict.Clear();
        if (emotions == null) return;

        for (int i = 0; i < emotions.Count; i++)
        {
            var e = emotions[i];
            if (e == EmotionType.None) continue;

            if (!dict.ContainsKey(e)) dict[e] = 0;
            dict[e]++;
        }
    }

    public MaskStage GetStageClamped(int stageIndex)
    {
        if (stages == null || stages.Count == 0) return null;
        if (stageIndex < 0) stageIndex = 0;
        if (stageIndex >= stages.Count) stageIndex = stages.Count - 1;
        return stages[stageIndex];
    }
}
