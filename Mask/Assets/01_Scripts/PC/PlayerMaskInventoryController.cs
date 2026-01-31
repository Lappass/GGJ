using UnityEngine;
using System.Collections.Generic;

public class PlayerMaskInventoryController : MonoBehaviour
{
    public static PlayerMaskInventoryController Instance { get; private set; }

    [Header("Unlocked Fragments")]
    // Now storing the Prefabs directly
    [SerializeField] private List<GameObject> unlockedFragments = new List<GameObject>();

    // Event to notify UI when inventory changes
    public event System.Action OnInventoryUpdated;

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
    
    // Debug method to test unlocking
    [ContextMenu("Unlock Random Test Fragment")]
    public void DebugUnlockTest()
    {
        // This would need a reference to a fragment to work, just a placeholder for logic
        OnInventoryUpdated?.Invoke();
    }
}

