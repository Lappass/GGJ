using UnityEngine;

public class FlowerPot : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("ID for saving state")]
    [SerializeField] private string objectID = "FlowerPot_01";
    [SerializeField] private float moveDistance = 1.0f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Tooltip("Only active after moving pot")]
    [SerializeField] private GameObject hiddenItem;

    private bool isPlayerInRange = false;
    private bool hasMoved = false;

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            hasMoved = GameStateManager.Instance.GetState(objectID);
        }

        if (hasMoved)
        {
            transform.Translate(Vector3.right * moveDistance);
            if (hiddenItem != null)
            {
                hiddenItem.SetActive(true);
                var col = hiddenItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
            }
        }
        else
        {
            if (hiddenItem != null)
            {
                var col = hiddenItem.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                hiddenItem.SetActive(true);
            }
        }
    }

    private void Update()
    {
        // Debug Logic
        if (Input.GetKeyDown(interactKey))
        {
            if (!isPlayerInRange)
                Debug.Log($"[FlowerPot] 按下 {interactKey}。不在交互范围内 (Trigger未触发?)。");
            else if (hasMoved)
                Debug.Log($"[FlowerPot] 按下 {interactKey}。但物体已移动过 (hasMoved=true)。");
            else
                Debug.Log($"[FlowerPot] 按下 {interactKey}。条件满足，尝试移动。");
        }

        if (isPlayerInRange && Input.GetKeyDown(interactKey) && !hasMoved)
        {
            MoveRight();
        }
    }

    private void MoveRight()
    {
        hasMoved = true;
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }
        
        transform.Translate(Vector3.right * moveDistance);

        // 移动后隐藏全局提示
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.HidePrompt();
        }

        if (hiddenItem != null)
        {
            var col = hiddenItem.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 增加 Debug Log
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
        {
            Debug.Log($"[FlowerPot] 玩家进入范围: {other.name}, Tag: {other.tag}");
            isPlayerInRange = true;
            if (!hasMoved && InteractionManager.Instance != null)
            {
                InteractionManager.Instance.ShowPrompt();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
        {
            Debug.Log($"[FlowerPot] 玩家离开范围: {other.name}");
            isPlayerInRange = false;
            if (InteractionManager.Instance != null)
            {
                InteractionManager.Instance.HidePrompt();
            }
        }
    }
}
