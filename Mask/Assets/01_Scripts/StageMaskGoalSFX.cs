using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StageMaskGoalSFX : MonoBehaviour
{
    [Header("Stage Requirement")]
    [SerializeField] private IdentityType requiredIdentity = IdentityType.None;

    [Tooltip("Stage requires these emotions to be present (subset match).")]
    [SerializeField] private List<EmotionType> requiredEmotions = new List<EmotionType>();

    [Tooltip("If true: emotions must match exactly (no extra emotions).")]
    [SerializeField] private bool requireExactEmotions = false;

    private bool _wasMatched = false;

    private void Start()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskStateChanged += OnMaskStateChanged;
            // 启动时立即评估一次（避免玩家进场已经满足但没触发）
            OnMaskStateChanged(MaskManager.Instance.CurrentIdentity, MaskManager.Instance.CurrentEmotions);
        }
        else
        {
            Debug.LogWarning("[StageMaskGoalSFX] MaskManager.Instance is null in Start()");
        }
    }

    private void OnDestroy()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskStateChanged -= OnMaskStateChanged;
        }
    }

    private void OnMaskStateChanged(IdentityType identity, IReadOnlyList<EmotionType> emotions)
    {
        bool matched = IsMatch(identity, emotions);

        // 只在 “从不满足 -> 满足” 的瞬间播放一次
        if (matched && !_wasMatched)
        {
            if (MaskAudio.Instance != null)
            {
                MaskAudio.Instance.PlaySuccess();
            }
            else
            {
                Debug.LogWarning("[StageMaskGoalSFX] MaskAudio.Instance is null, cannot play success SFX");
            }
        }

        _wasMatched = matched;
    }

    private bool IsMatch(IdentityType identity, IReadOnlyList<EmotionType> emotions)
    {
        // Identity 必须相等（你的需求是 “对应stage要求的identity对上”）
        if (identity != requiredIdentity) return false;

        // required emotions 必须都包含
        for (int i = 0; i < requiredEmotions.Count; i++)
        {
            if (!emotions.Contains(requiredEmotions[i]))
                return false;
        }

        // 可选：要求情绪集合完全一致（无额外情绪）
        if (requireExactEmotions)
        {
            // 允许 requiredEmotions 里不重复；emotions 也视作集合比较
            if (emotions.Count != requiredEmotions.Count) return false;
        }

        return true;
    }

    // 如果你 stage 会动态切换需求（下一关/下一阶段），可以调用这个：
    public void SetGoal(IdentityType newIdentity, List<EmotionType> newEmotions, bool exact)
    {
        requiredIdentity = newIdentity;
        requiredEmotions = newEmotions ?? new List<EmotionType>();
        requireExactEmotions = exact;

        _wasMatched = false; // 重置，允许新目标达成时再响一次

        if (MaskManager.Instance != null)
            OnMaskStateChanged(MaskManager.Instance.CurrentIdentity, MaskManager.Instance.CurrentEmotions);
    }
}
