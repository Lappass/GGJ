using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    [Tooltip("The panel GameObject to toggle on/off")]
    [SerializeField] private GameObject targetPanel;
    
    [Tooltip("The key used to toggle the panel")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    
    [Tooltip("Should the panel be visible when the game starts?")]
    [SerializeField] private bool startActive = false;

    private void Start()
    {
        if (targetPanel == null)
        {
            Debug.LogWarning("TogglePanel: Target Panel is not assigned. Please assign it in the inspector.");
        }
        else
        {
            targetPanel.SetActive(startActive);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (targetPanel != null)
        {
            if (targetPanel.activeSelf)
            {
                CleanupDraggables(targetPanel.transform);
            }
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
    }

    private void CleanupDraggables(Transform panelRoot)
    {
        // 1. Handle items spawned from InventoryItemIcon
        InventoryItemIcon[] icons = panelRoot.GetComponentsInChildren<InventoryItemIcon>(true);
        foreach (var icon in icons)
        {
            icon.CancelDrag();
        }

        // 2. Handle items moved from slots (DraggableUI)
        // Find all active draggables in the scene (since dragged items are reparented to root, they are active)
        DraggableUI[] allDraggables = FindObjectsOfType<DraggableUI>();
        foreach (var drag in allDraggables)
        {
            // If the item came from a slot inside this panel
            if (drag.parentBeforeDrag != null && drag.parentBeforeDrag.IsChildOf(panelRoot))
            {
                // And it's currently floating (not in its original spot)
                if (drag.transform.parent != drag.parentBeforeDrag)
                {
                    drag.ReturnToPreviousParent();
                }
            }
        }
    }
}




