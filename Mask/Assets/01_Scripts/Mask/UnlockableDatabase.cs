using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnlockableDatabase", menuName = "Mask/Unlockable Database")]
public class UnlockableDatabase : ScriptableObject
{
    [System.Serializable]
    public class UnlockEntry
    {
        public string id; // Unique ID (e.g., "Identity_Detective", "Emotion_Happy")
        public string displayName; // "Detective", "Joy"
        public Sprite icon; // Unique icon for this type
        public AttributeType type; // Helper to categorize
        public IdentityType identityType; // Match with Enum
        public EmotionType emotionType;   // Match with Enum
    }

    public List<UnlockEntry> entries = new List<UnlockEntry>();

    public UnlockEntry GetEntry(IdentityType idType)
    {
        return entries.Find(e => e.type == AttributeType.Identity && e.identityType == idType);
    }

    public UnlockEntry GetEntry(EmotionType emoType)
    {
        return entries.Find(e => e.type == AttributeType.Emotion && e.emotionType == emoType);
    }
}

