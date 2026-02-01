using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("唯一ID")]
    [SerializeField] private string objectID = "Item_01";
    [Header("Reward")]
    [Tooltip("If set, picking up this item will unlock this mask fragment PREFAB.")]
    [SerializeField] private System.Collections.Generic.List<GameObject> fragmentRewards;
    [SerializeField] private GameObject fragmentReward;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("Behavior")]
    [Tooltip("Should the object be destroyed immediately when picked up? Uncheck this if you want to play dialogue first.")]
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent onPickedUp;

    private bool isPlayerInRange = false;

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            bool isPicked = GameStateManager.Instance.GetState(objectID);
            if (isPicked)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            PickUp();
        }
    }

    private void PickUp()
    {
        Debug.Log($"捡起了物品: {gameObject.name}");
        
        // Unlock Fragment Reward
        if (PlayerMaskInventoryController.Instance != null)
        {
             if (fragmentReward != null)
                 PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
                 
             if (fragmentRewards != null)
             {
                 foreach(var f in fragmentRewards)
                 {
                     if (f != null) PlayerMaskInventoryController.Instance.UnlockFragment(f);
                 }
             }
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }

        onPickedUp?.Invoke();

        // 捡起后隐藏提示
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.HidePrompt();
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            // If not destroying, we should probably disable collider/visuals or at least disable this script
            // so it can't be picked up again immediately?
            // For now, let's just disable the collider to prevent re-triggering
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            
            // Also hide prompt just in case
            isPlayerInRange = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
        {
            isPlayerInRange = true;
            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.ShowPrompt();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
        {
            isPlayerInRange = false;
            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.HidePrompt();
            }
        }
    }
}
