using System.Collections.Generic;

public static class MaskStageProgressRuntime
{
    private static readonly Dictionary<string, int> _stageByKey = new Dictionary<string, int>();
    private static readonly HashSet<string> _rewardFlags = new HashSet<string>();

    public static int GetStage(string key)
    {
        if (string.IsNullOrEmpty(key)) return 0;
        return _stageByKey.TryGetValue(key, out int v) ? v : 0;
    }

    public static void SetStage(string key, int stageIndex)
    {
        if (string.IsNullOrEmpty(key)) return;
        _stageByKey[key] = stageIndex;
    }

    public static bool HasGrantedReward(string key, int stageIndex)
    {
        return _rewardFlags.Contains(BuildRewardKey(key, stageIndex));
    }

    public static void MarkRewardGranted(string key, int stageIndex)
    {
        _rewardFlags.Add(BuildRewardKey(key, stageIndex));
    }

    public static void ResetAll()
    {
        _stageByKey.Clear();
        _rewardFlags.Clear();
    }

    private static string BuildRewardKey(string key, int stageIndex)
    {
        return $"{key}__reward__{stageIndex}";
    }
}
