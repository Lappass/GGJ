using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    public IdentityType CurrentIdentity { get; private set; } = (IdentityType)0;
    public System.Collections.Generic.List<EmotionType> CurrentEmotions { get; private set; } = new System.Collections.Generic.List<EmotionType>();


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

        for (int i = 0; i < maskSlots.Count; i++)
        {
            var slot = maskSlots[i];
            if (slot != null && slot.currentItem != null && slot.currentItem.attributeData != null)
            {
                // Strict validation:
                // If the slot defines an accepted position, the item MUST match it.
                if (slot.acceptedPosition != MaskPosition.None && 
                    slot.currentItem.positionType != slot.acceptedPosition)
                {
                    Debug.LogWarning($"Slot {slot.name} has item {slot.currentItem.name} with mismatched position type. Ignoring attributes.");
                    continue;
                }

                // Fallback validation based on index if slot is None (generic):
                // REMOVED: This was causing issues if slots were not in expected 0-3 order or if placement was loose.
                // We trust that if an item is in a slot, it counts.
                /* 
                if (slot.acceptedPosition == MaskPosition.None && maskSlots.Count == 4)
                {
                    MaskPosition expectedPos = MaskPosition.None;
                    switch (i)
                    {
                        case 0: expectedPos = MaskPosition.TopLeft; break;
                        case 1: expectedPos = MaskPosition.TopRight; break;
                        case 2: expectedPos = MaskPosition.BottomLeft; break;
                        case 3: expectedPos = MaskPosition.BottomRight; break;
                    }

                    if (slot.currentItem.positionType != MaskPosition.None && 
                        slot.currentItem.positionType != expectedPos)
                    {
                         Debug.LogWarning($"Slot index {i} (implicitly {expectedPos}) has item {slot.currentItem.name} ({slot.currentItem.positionType}). Ignoring attributes.");
                         continue;
                    }
                }
                */

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

        // 3. Logic for Thresholds
        // Detective: 3, Journalist: 2, Therapist: 1, DirtyCop: 4
        
        // DEBUG: Print counts
        string debugCounts = "Counts: ";
        foreach(var kvp in identityCounts) debugCounts += $"{kvp.Key}:{kvp.Value} ";
        Debug.Log(debugCounts);

        IdentityType finalIdentity = IdentityType.None;  
        bool identityFound = false;

        if (identityCounts.ContainsKey(IdentityType.DirtyCop) && identityCounts[IdentityType.DirtyCop] >= 4)
        {
            finalIdentity = IdentityType.DirtyCop;
            identityFound = true;
        }
        else if (identityCounts.ContainsKey(IdentityType.Detective) && identityCounts[IdentityType.Detective] >= 3)
        {
            finalIdentity = IdentityType.Detective;
            identityFound = true;
        }
        else if (identityCounts.ContainsKey(IdentityType.Journalist) && identityCounts[IdentityType.Journalist] >= 2)
        {
            finalIdentity = IdentityType.Journalist;
            identityFound = true;
        }
        else if (identityCounts.ContainsKey(IdentityType.Therapist) && identityCounts[IdentityType.Therapist] >= 1)
        {
            finalIdentity = IdentityType.Therapist;
            identityFound = true;
        }

        // Emotions: "One counts as one"
        List<EmotionType> finalEmotions = new List<EmotionType>();
        foreach(var kvp in emotionCounts)
        {
            if (kvp.Value > 0)
            {
                // If you want "3 Happy" to mean "Happy x3", we store them distinct?
                // The user said "one counts as one", so if I have 3 happy fragments, do I have "Happy" state?
                // Usually yes. Or do I list all?
                // "情绪就是有一个算一个" -> Sounds like we just list them present.
                finalEmotions.Add(kvp.Key);
            }
        }

        // 4. Update Player State
        if (PlayerMaskInventoryController.Instance != null)
        {
            if (identityFound)
            {
               PlayerMaskInventoryController.Instance.UpdateMaskState(finalIdentity, finalEmotions);
            }
            else
            {
                // Pass None now that we have it
                PlayerMaskInventoryController.Instance.UpdateMaskState(IdentityType.None, finalEmotions); 
            }
        }

        // Save current mask state for dialogue resolution
        CurrentIdentity = identityFound ? finalIdentity : (IdentityType)0;
        CurrentEmotions.Clear();
        CurrentEmotions.AddRange(finalEmotions);

        if (CurrentEmotions.Count > 3)
            CurrentEmotions.RemoveRange(3, CurrentEmotions.Count - 3);

        // 5. Build string for Display
        StringBuilder sb = new StringBuilder();
        sb.Append("Current Mask: ");

        List<string> parts = new List<string>();

        if (identityFound)
        {
            parts.Add($"{finalIdentity}");
        }
        else
        {
            parts.Add("Unknown Identity");
        }

        foreach (var emo in finalEmotions)
        {
            parts.Add(emo.ToString());
        }
        
        // Debug raw counts
        // foreach (var kvp in identityCounts) { if(kvp.Value > 0) parts.Add($"({kvp.Key} x{kvp.Value})"); }

        sb.Append(string.Join(", ", parts));

        // 6. Send to display
        if (MaskAttributeDisplay.Instance != null)
        {
            MaskAttributeDisplay.Instance.ShowAttributes(sb.ToString());
        }
    }
}

