using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    public GameObject realPrefabToSpawn; // The actual DraggableUI prefab
    
    private GameObject currentDragObject;
    private DraggableUI currentDraggable;
    private Canvas rootCanvas;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rootCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        }
    }

    public void Setup(GameObject prefab, Sprite icon)
    {
        realPrefabToSpawn = prefab;
        // Find Image in children if it's not on the root
        Image img = GetComponent<Image>();
        if (img == null) img = GetComponentInChildren<Image>();
        
        if (img != null && icon != null) 
        {
            img.sprite = icon;
            // Ensure alpha is 1 in case prefab default is low
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        // Ensure CanvasGroup is visible
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (realPrefabToSpawn == null) return;

        // Hide the icon while dragging
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 1. Spawn the REAL draggable object on the root canvas
        currentDragObject = Instantiate(realPrefabToSpawn, rootCanvas.transform);
        currentDragObject.transform.position = transform.position; // Start at icon position
        
        // 2. Initialize it
        currentDraggable = currentDragObject.GetComponent<DraggableUI>();
        if (currentDraggable != null)
        {
            // Set original prefab so we can return it to inventory later
            currentDraggable.originalPrefab = realPrefabToSpawn;

            // Tell it that it doesn't have a "home" parent in the layout group.
            // It is a temporary object until placed.
            currentDraggable.returnToStartPosOnRelease = false; 
            
            // Manually trigger its drag start
            // We need to set pointerDrag on eventData so ItemSlot can find it
            eventData.pointerDrag = currentDragObject;

            // Subscribe to the end drag event since we gave away control
            currentDraggable.onEndDragCallback += OnChildEndDrag;

            currentDraggable.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // This might not be called if pointerDrag was swapped, but good to keep just in case
        if (currentDraggable != null)
        {
            currentDraggable.OnDrag(eventData);
        }
    }

    // This is called by DraggableUI via callback
    private void OnChildEndDrag(PointerEventData eventData)
    {
        if (currentDraggable != null)
        {
            // Unsubscribe
            currentDraggable.onEndDragCallback -= OnChildEndDrag;

            // Check if it was successfully placed
            // We can check if its parent is now an ItemSlot
            if (currentDragObject != null && currentDragObject.transform.parent != null && 
                currentDragObject.transform.parent.GetComponent<ItemSlot>() != null)
            {
                // Success! It found a home.
                // Remove from inventory so it doesn't reappear on refresh
                if (PlayerMaskInventoryController.Instance != null)
                {
                    PlayerMaskInventoryController.Instance.RemoveFragment(realPrefabToSpawn);
                }
                
                // Destroy the icon from inventory since the item is now in the world
                Destroy(gameObject);
            }
            else
            {
                // Failed to place (dropped in void or invalid slot)
                if (currentDragObject != null)
                {
                    Destroy(currentDragObject);
                }
                
                // Show icon again
                CanvasGroup cg = GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
            
            currentDragObject = null;
            currentDraggable = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // This is unlikely to be called if pointerDrag was swapped, 
        // but we implement it to satisfy interface
    }

    public void CancelDrag()
    {
        if (currentDragObject != null)
        {
            Destroy(currentDragObject);
            currentDragObject = null;
            currentDraggable = null;
        }
        
        // Show icon again
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
    }
}

