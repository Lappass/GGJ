using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    // The item currently held by this slot
    public DraggableUI currentItem;

    public void OnDrop(PointerEventData eventData)
    {
        // Check if the dropped object is a DraggableUI
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
        // Double click to eject
        if (eventData.clickCount == 2 && currentItem != null)
        {
            EjectItem();
        }
    }

    private void PlaceItem(DraggableUI item)
    {
        // If there's already an item, eject it first
        if (currentItem != null)
        {
            if (currentItem == item) return; // Dropped on self
            EjectItem();
        }

        // Snap the new item to this slot
        currentItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero; // Center it
        
        // Ensure it's centered
        RectTransform itemRect = item.GetComponent<RectTransform>();
        if (itemRect != null)
        {
            itemRect.anchoredPosition = Vector2.zero;
        }

        Debug.Log($"Slot {name} absorbed {item.name}");
    }

    private void EjectItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"Slot {name} ejecting {currentItem.name}");
            
            // Return to its original home/inventory
            currentItem.ReturnToHome();
            
            currentItem = null;
        }
    }

    public void Clear()
    {
        currentItem = null;
    }
}

