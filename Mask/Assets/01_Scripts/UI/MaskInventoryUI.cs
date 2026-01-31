using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MaskInventoryUI : MonoBehaviour
{
    [Header("References")]
    public Transform contentParent; // The Grid/Layout Group to spawn items into
    
    // We no longer need the list of shapes because we instantiate the prefab directly from inventory
    // [Header("Fragment Prefabs")]
    // [Tooltip("List of prefabs for different fragment shapes. The script will pick the one matching the Data's position.")]
    // public List<GameObject> fragmentPrefabs;

    private void Start()
    {
        // Subscribe to inventory updates
        if (PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnInventoryUpdated += RefreshInventory;
            // Initial refresh
            RefreshInventory();
        }
    }

    private void OnDestroy()
    {
        if (PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.OnInventoryUpdated -= RefreshInventory;
        }
    }

    public void RefreshInventory()
    {
        if (contentParent == null)
        {
            Debug.LogError("MaskInventoryUI: Content Parent is not assigned!");
            return;
        }

        if (PlayerMaskInventoryController.Instance == null)
        {
            Debug.LogWarning("MaskInventoryUI: PlayerMaskInventoryController Instance is null.");
            return;
        }

        // 1. Clear existing items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Spawn new items based on inventory
        var unlockedFragments = PlayerMaskInventoryController.Instance.GetUnlockedFragments();
        
        if (unlockedFragments != null)
        {
            Debug.Log($"MaskInventoryUI: Refreshing inventory with {unlockedFragments.Count} items.");
            foreach (var prefab in unlockedFragments)
            {
                if (prefab != null)
                {
                    // Instantiate the prefab directly
                    // false ensures worldPositionStays is false, keeping local transform relative to parent
                    GameObject instance = Instantiate(prefab, contentParent, false); 
                }
            }
        }
    }
}

