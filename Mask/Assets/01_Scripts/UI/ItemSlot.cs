using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("Slot Settings")]
    [Tooltip("The position type this slot accepts. Set to None to accept everything (or handle differently).")]
    public MaskPosition acceptedPosition = MaskPosition.None;

    // The item currently held by this slot
    public DraggableUI currentItem;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableUI draggable = eventData.pointerDrag.GetComponent<DraggableUI>();
            if (draggable != null)
            {
                PlaceItem(draggable);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2 && currentItem != null)
        {
            EjectItem();
        }
    }

    private void PlaceItem(DraggableUI item)
    {
        // Check if types match
        if (acceptedPosition != MaskPosition.None && item.positionType != acceptedPosition)
        {
            Debug.Log($"Type mismatch: Slot expects {acceptedPosition}, Item is {item.positionType}");
            // Optional: Provide visual feedback for failure?
            // Since we don't snap it, it will just drop in place (or return home if configured)
            return; 
        }

        // If there's already an item, eject it first
        if (currentItem != null)
        {
            if (currentItem == item) return;
            EjectItem();
        }

        currentItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero; 
        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            itemRect.anchoredPosition = Vector2.zero;
        }

        Debug.Log($"Slot {name} absorbed {item.name}");

        // Notify Manager to update total attributes
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskContentChanged();
        }
    }

    private void EjectItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"Slot {name} ejecting {currentItem.name}");
            currentItem.ReturnToHome();
            
            currentItem = null;

            // Notify Manager
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.OnMaskContentChanged();
            }
        }
    }

    public void Clear()
    {
        currentItem = null;
        // Notify Manager
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskContentChanged();
        }
    }
}

