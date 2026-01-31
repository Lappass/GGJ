using UnityEngine;
using System.Collections.Generic;

public class FragmentRewarder : MonoBehaviour
{
    [Header("Rewards")]
    [Tooltip("List of fragment prefabs to grant when TriggerRewards is called.")]
    public List<GameObject> fragmentsToGrant;

    [Header("Settings")]
    [Tooltip("Should this GameObject be destroyed after granting rewards?")]
    public bool destroyAfterGrant = false;

    // Call this method from your Dialogue System's "On Dialogue End" event
    public void GrantRewards()
    {
        if (PlayerMaskInventoryController.Instance == null)
        {
            Debug.LogWarning("PlayerMaskInventoryController is missing!");
            return;
        }

        int count = 0;
        foreach (var prefab in fragmentsToGrant)
        {
            if (prefab != null)
            {
                PlayerMaskInventoryController.Instance.UnlockFragment(prefab);
                count++;
            }
        }
        
        Debug.Log($"FragmentRewarder: Granted {count} fragments.");

        if (destroyAfterGrant)
        {
            Destroy(gameObject);
        }
    }
}

