using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            DraggableUI draggable = eventData.pointerDrag.GetComponent<DraggableUI>();
            if (draggable != null && draggable.originalPrefab != null)
            {
                // Restore to inventory
                if (PlayerMaskInventoryController.Instance != null)
                {
                    PlayerMaskInventoryController.Instance.UnlockFragment(draggable.originalPrefab);
                }

                // If it was in a slot, clear the slot (though DraggableUI logic should handle parent checks)
                // But DraggableUI.OnEndDrag might try to return to parent if we don't handle it carefully.
                // However, since we are destroying it immediately, OnEndDrag on DraggableUI will run on a destroyed object or be interrupted?
                // Actually OnEndDrag runs after OnDrop.
                // If we destroy it here, OnEndDrag might throw errors or not run.
                
                // Safe way: Destroy it.
                Destroy(draggable.gameObject);
                
                // If the draggable was dragged from a slot, the slot reference is in parentBeforeDrag.
                // But DraggableUI.OnBeginDrag sets parent to root canvas.
                // The slot (if any) called Clear() in OnBeginDrag so it's already empty.
                // So destroying it here is safe.
            }
        }
    }
}




