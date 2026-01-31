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
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
    }
}

