using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    [Header("Slots")]
    // Assign the 4 slots in Inspector
    [SerializeField] private List<ItemSlot> maskSlots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to events if slots had them, but currently slots call us directly or we poll them.
        // Let's have a method that slots call when they change.
        UpdateMaskAttributes();
    }

    public void OnMaskContentChanged()
    {
        UpdateMaskAttributes();
    }

    private void UpdateMaskAttributes()
    {
        // 1. Calculate counts
        Dictionary<EmotionType, int> emotionCounts = new Dictionary<EmotionType, int>();
        Dictionary<IdentityType, int> identityCounts = new Dictionary<IdentityType, int>();
        int totalItems = 0;

        foreach (var slot in maskSlots)
        {
            if (slot != null && slot.currentItem != null && slot.currentItem.attributeData != null)
            {
                MaskAttributeData data = slot.currentItem.attributeData;
                totalItems++;

                if (data.type == AttributeType.Emotion)
                {
                    if (!emotionCounts.ContainsKey(data.emotionValue)) emotionCounts[data.emotionValue] = 0;
                    emotionCounts[data.emotionValue]++;
                }
                else if (data.type == AttributeType.Identity)
                {
                    if (!identityCounts.ContainsKey(data.identityValue)) identityCounts[data.identityValue] = 0;
                    identityCounts[data.identityValue]++;
                }
            }
        }

        // 2. Hide text if no items
        if (MaskAttributeDisplay.Instance != null)
        {
            if (totalItems == 0)
            {
                MaskAttributeDisplay.Instance.ClearAttributes();
                return;
            }
        }

        // 3. Build string: e.g. "Detective x3, Angry x1"
        StringBuilder sb = new StringBuilder();
        sb.Append("Current Mask: ");

        List<string> parts = new List<string>();

        // Identities first
        foreach (var kvp in identityCounts)
        {
            if (kvp.Value > 0)
                parts.Add($"{kvp.Key} x{kvp.Value}");
        }

        // Emotions second
        foreach (var kvp in emotionCounts)
        {
            if (kvp.Key != EmotionType.None || kvp.Value > 0)
                parts.Add($"{kvp.Key} x{kvp.Value}");
        }

        sb.Append(string.Join(", ", parts));

        // 4. Send to display
        if (MaskAttributeDisplay.Instance != null)
        {
            MaskAttributeDisplay.Instance.ShowAttributes(sb.ToString());
        }
    }
}

