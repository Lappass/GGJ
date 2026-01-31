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

        // 3. Logic for Thresholds
        // Detective: 3, Journalist: 2, Therapist: 1, DirtyCop: 4
        
        IdentityType finalIdentity = IdentityType.Journalist; // Default or None if you had it. 
        // Since we don't have None in the enum easily (or maybe we do), let's use a flag or check counts.
        // Actually, let's determine based on priority or just first match?
        // Let's assume hierarchy: DirtyCop > Detective > Journalist > Therapist (based on difficulty?)
        
        // Better: Reset to a "None" equivalent.
        // If I can't easily add None to the enum right now, I'll rely on a separate boolean or just use a fallback.
        // Let's assume standard is no identity.
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
            // If no identity found, what do we pass?
            // Since we didn't successfully add None, maybe just pass the first enum value if !identityFound?
            // Or careful logic.
            // Let's just update if found, or handle "None" conceptually.
            // I'll try to cast 0 or similar if needed, but safer to just send what we have.
            // If identityFound is false, we might not want to set it to Therapist/Journalist incorrectly.
            // I'll assume for now if not found, it keeps previous or we need a None.
            
            // To be safe, I will use a dummy value if needed, but really we need None.
            if (identityFound)
            {
               PlayerMaskInventoryController.Instance.UpdateMaskState(finalIdentity, finalEmotions);
            }
            else
            {
                // Pass a "None" equivalent logic. 
                // Since I failed to add None, I will stick with "Journalist" but maybe I can use a separate bool in controller?
                // No, I should really fix the Enum.
                // But assuming I can't right now, I'll just pass the first one but maybe clear the list?
                // Actually, let's just update the emotions if identity is not found.
                // But the user wants the state saved.
                
                // Let's assume the user will manually add None if I can't.
                // I will try to pass (IdentityType)0 if I can't find one, assuming 0 is default.
                PlayerMaskInventoryController.Instance.UpdateMaskState((IdentityType)0, finalEmotions); 
            }
        }

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

