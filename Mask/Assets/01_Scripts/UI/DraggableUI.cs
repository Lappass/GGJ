using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public Transform parentBeforeDrag;
    public Transform homeParent;
    public bool returnToStartPosOnRelease = false;
    
    [Header("Item Properties")]
    public MaskPosition positionType = MaskPosition.None;
    
    [Tooltip("The attribute data for this mask fragment")]
    public MaskAttributeData attributeData;

    // Optional flag if you need to know if this was spawned dynamically
    [HideInInspector] public bool isSpawnedFromInventory = false; 

    private Vector2 positionBeforeDrag;
    private Transform rootCanvasTransform;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount >= 2)
        {
            // If we are inside a slot, tell the slot to destroy us
            if (transform.parent != null)
            {
                ItemSlot slot = transform.parent.GetComponent<ItemSlot>();
                if (slot != null && slot.currentItem == this)
                {
                    slot.OnItemDoubleClicked();
                    return; // Handled by slot
                }
            }
            
            // If not in a slot (e.g. floating or just dragged out), destroy self directly
            // This is useful if it got stuck or user just wants to cancel it
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        homeParent = transform.parent;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            rootCanvasTransform = canvas.rootCanvas != null ? canvas.rootCanvas.transform : canvas.transform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentBeforeDrag = transform.parent;
        ItemSlot slot = parentBeforeDrag.GetComponent<ItemSlot>();
        if (slot != null && slot.currentItem == this)
        {
            slot.Clear();
        }

        positionBeforeDrag = rectTransform.anchoredPosition;
        if (rootCanvasTransform != null)
        {
            transform.SetParent(rootCanvasTransform);
        }
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvasTransform != null)
        {
             // Adjust position based on canvas scale
             Canvas canvas = rootCanvasTransform.GetComponent<Canvas>();
             if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
             {
                 rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
             }
             else
             {
                 transform.position = Input.mousePosition;
             }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        // If the parent is still the root canvas
        if (transform.parent == rootCanvasTransform && returnToStartPosOnRelease)
        {
            ReturnToPreviousParent();
        }
    }

    public void ReturnToPreviousParent()
    {
        if (parentBeforeDrag != null)
        {
            transform.SetParent(parentBeforeDrag);
            rectTransform.anchoredPosition = positionBeforeDrag;
        }
    }

    public void ReturnToHome()
    {
        if (homeParent != null)
        {
            transform.SetParent(homeParent);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}

