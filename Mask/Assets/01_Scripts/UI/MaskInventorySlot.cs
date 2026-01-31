using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Data")]
    public GameObject prefabReference; // The DraggableUI prefab this slot represents
    public Sprite icon;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private CanvasGroup canvasGroup; // Optional, for visual feedback

    // Internal state
    private GameObject currentDraggedObject;
    private Transform rootCanvasTransform;

    private void Start()
    {
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        // Find root canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rootCanvasTransform = canvas.rootCanvas != null ? canvas.rootCanvas.transform : canvas.transform;
        }
    }

    public void Setup(GameObject prefab, Sprite sprite)
    {
        prefabReference = prefab;
        icon = sprite;
        if (iconImage != null) iconImage.sprite = icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (prefabReference == null) return;

        // 1. Instantiate the actual draggable prefab
        // We spawn it on the root canvas so it floats above everything
        currentDraggedObject = Instantiate(prefabReference, rootCanvasTransform);
        
        // 2. Setup the draggable object
        DraggableUI draggable = currentDraggedObject.GetComponent<DraggableUI>();
        if (draggable != null)
        {
            // Configure it to handle the drag immediately
            // We need to manually initialize the drag on the new object because it missed the OnBeginDrag event
            draggable.isSpawnedFromInventory = true; // Tell it it's a temporary spawn
            draggable.OnBeginDrag(eventData); 
        }

        // 3. Visual feedback for the slot (optional)
        if (canvasGroup != null) canvasGroup.alpha = 0.5f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentDraggedObject != null)
        {
            // Forward the drag event to the spawned object
            DraggableUI draggable = currentDraggedObject.GetComponent<DraggableUI>();
            if (draggable != null)
            {
                draggable.OnDrag(eventData);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentDraggedObject != null)
        {
            // Forward the end drag event
            DraggableUI draggable = currentDraggedObject.GetComponent<DraggableUI>();
            if (draggable != null)
            {
                draggable.OnEndDrag(eventData);
            }
            
            // Forget the object
            currentDraggedObject = null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }
}

