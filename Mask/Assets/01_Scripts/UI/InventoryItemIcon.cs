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
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (realPrefabToSpawn == null) return;

        // 1. Spawn the REAL draggable object on the root canvas
        currentDragObject = Instantiate(realPrefabToSpawn, rootCanvas.transform);
        currentDragObject.transform.position = transform.position; // Start at icon position
        
        // 2. Initialize it
        currentDraggable = currentDragObject.GetComponent<DraggableUI>();
        if (currentDraggable != null)
        {
            // Tell it that it doesn't have a "home" parent in the layout group.
            // It is a temporary object until placed.
            currentDraggable.returnToStartPosOnRelease = false; 
            
            // Manually trigger its drag start
            // We need to set pointerDrag on eventData so ItemSlot can find it
            eventData.pointerDrag = currentDragObject;
            currentDraggable.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentDraggable != null)
        {
            currentDraggable.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentDraggable != null)
        {
            currentDraggable.OnEndDrag(eventData);

            // Check if it was successfully placed
            // We can check if its parent is now an ItemSlot
            if (currentDragObject.transform.parent.GetComponent<ItemSlot>() != null)
            {
                // Success! It found a home.
                // Destroy the icon from inventory since the item is now in the world
                Destroy(gameObject);
            }
            else
            {
                // Failed to place (dropped in void or invalid slot)
                Destroy(currentDragObject);
            }
            
            currentDragObject = null;
            currentDraggable = null;
        }
    }
}

