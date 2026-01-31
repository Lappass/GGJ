using UnityEngine;
using System.Collections.Generic;

public class PlayerMaskInventoryController : MonoBehaviour
{
    public static PlayerMaskInventoryController Instance { get; private set; }

    [Header("Unlocked Fragments")]
    // Now storing the Prefabs directly
    [SerializeField] private List<GameObject> unlockedFragments = new List<GameObject>();

    [Header("Current Mask State")]
    public IdentityType CurrentIdentity { get; private set; }
    public List<EmotionType> CurrentEmotions { get; private set; } = new List<EmotionType>();

    // Event to notify UI when inventory changes
    public event System.Action OnInventoryUpdated;
    // Event when the mask configuration changes (identity/emotions updated)
    public event System.Action OnMaskStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<GameObject> GetUnlockedFragments()
    {
        return unlockedFragments;
    }

    public void UnlockFragment(GameObject fragmentPrefab)
    {
        if (fragmentPrefab != null && !unlockedFragments.Contains(fragmentPrefab))
        {
            unlockedFragments.Add(fragmentPrefab);
            Debug.Log($"Unlocked fragment: {fragmentPrefab.name}");
            
            // Notify listeners (UI)
            OnInventoryUpdated?.Invoke();
        }
    }

    public void RemoveFragment(GameObject fragmentPrefab)
    {
        if (fragmentPrefab != null && unlockedFragments.Contains(fragmentPrefab))
        {
            unlockedFragments.Remove(fragmentPrefab);
            
            OnInventoryUpdated?.Invoke();
        }
    }
    
    public void UpdateMaskState(IdentityType identity, List<EmotionType> emotions)
    {
        CurrentIdentity = identity;
        CurrentEmotions = emotions ?? new List<EmotionType>();
        
        Debug.Log($"Mask State Updated: Identity={CurrentIdentity}, Emotions={string.Join(",", CurrentEmotions)}");
        OnMaskStateChanged?.Invoke();
    }

    public void ForceRefreshUI()
    {
        OnInventoryUpdated?.Invoke();
    }
}

