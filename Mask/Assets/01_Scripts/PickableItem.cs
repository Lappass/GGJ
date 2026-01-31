using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("唯一ID")]
    [SerializeField] private string objectID = "Item_01";
    [Header("Reward")]
    [Tooltip("If set, picking up this item will unlock this mask fragment PREFAB.")]
    [SerializeField] private GameObject fragmentReward;

    [SerializeField] private KeyCode interactKey = KeyCode.E;

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
        if (fragmentReward != null && PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
        }

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }

        // 捡起后隐藏提示
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.HidePrompt();
        }

        Destroy(gameObject);
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
