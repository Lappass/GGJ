using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public enum SlotType
    {
        Backpack,   // 背包/库存区域
        Assemble    // 中间拼接面具区域
    }

    [Header("Slot Settings")]
    [Tooltip("The position type this slot accepts. Set to None to accept everything (or handle differently).")]
    public MaskPosition acceptedPosition = MaskPosition.None;

    [Tooltip("Backpack = inventory area, Assemble = center mask sockets.")]
    public SlotType slotType = SlotType.Backpack;

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
            DestroyItem();
        }
    }

    // Called by DraggableUI when IT is double clicked
    public void OnItemDoubleClicked()
    {
        DestroyItem();
    }

    private void DestroyItem()
    {
        if (currentItem != null)
        {
            Debug.Log($"Slot {name} destroying {currentItem.name}");

            // Capture original prefab before destroying
            GameObject prefabToRestore = currentItem.originalPrefab;

            // Destroy object -> Inventory Controller Refresh -> Icon reappears in inventory
            Destroy(currentItem.gameObject);
            currentItem = null;

            // Notify Manager that mask content changed
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.OnMaskContentChanged();
            }

            // Restore to inventory
            if (PlayerMaskInventoryController.Instance != null && prefabToRestore != null)
            {
                PlayerMaskInventoryController.Instance.UnlockFragment(prefabToRestore);
            }
            else if (PlayerMaskInventoryController.Instance != null)
            {
                PlayerMaskInventoryController.Instance.ForceRefreshUI();
            }
        }
    }

    private void EjectItem()
    {
        DestroyItem();
    }

    private void PlaceItem(DraggableUI item)
    {
        // Check if types match
        if (acceptedPosition != MaskPosition.None && item.positionType != acceptedPosition)
        {
            Debug.Log($"Type mismatch: Slot expects {acceptedPosition}, Item is {item.positionType}");
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

        PlayPlaceSfx(item);

        // Notify Manager to update total attributes
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskContentChanged();
        }
    }

    private void PlayPlaceSfx(DraggableUI item)
    {
        if (item == null) return;
        if (MaskAudio.Instance == null) return;

        // 关键：你要从 DraggableUI 拿到 attributeData
        // 你队友的 MaskManager 用 slot.currentItem.attributeData
        // 所以 DraggableUI 里必须有 attributeData 字段
        var data = item.attributeData; // <-- 这里如果报错，说明 DraggableUI 没有 attributeData（看下面“常见报错”）
        if (data == null) return;

        // Backpack：只随机 5 个 putback
        if (slotType == SlotType.Backpack)
        {
            MaskAudio.Instance.PlayOnAttach(false, data);
            return;
        }

        // Assemble：Emotion -> 专属；Identity -> 随机 putback
        if (slotType == SlotType.Assemble)
        {
            MaskAudio.Instance.PlayOnAttach(true, data);
            return;
        }
    }

    public void Clear()
    {
        currentItem = null;
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskContentChanged();
        }
    }
}
