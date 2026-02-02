using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    [Header("Audio")]
    [Tooltip("Play random putback SFX when a mask is returned to backpack successfully.")]
    [SerializeField] private bool playSfxOnReturn = true;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableUI draggable = eventData.pointerDrag.GetComponent<DraggableUI>();
        if (draggable == null) return;

        // 只有“确实是可回收的碎片”（有 originalPrefab）才算放回成功
        if (draggable.originalPrefab == null) return;

        if (playSfxOnReturn && MaskAudio.Instance != null)
        {
            MaskAudio.Instance.PlayOnAttach(false, null);
            Debug.Log("[Mask SFX] Returned to backpack -> PlayReturnRandom()");
        }

        if (PlayerMaskInventoryController.Instance != null)
        {
            PlayerMaskInventoryController.Instance.UnlockFragment(draggable.originalPrefab);
        }
        else
        {
            Debug.LogWarning("PlayerMaskInventoryController.Instance is null!");
        }

        Destroy(draggable.gameObject);
    }
}
