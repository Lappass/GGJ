using UnityEngine;
using TMPro;

public class MaskAttributeDisplay : MonoBehaviour
{
    public static MaskAttributeDisplay Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI attributeText;
    [SerializeField] private GameObject background;
    [SerializeField] private Vector3 offset = new Vector3(20, -20, 0);
    [Tooltip("If true, the tooltip follows the mouse every frame. If false, it stays at the position where it first appeared.")]
    [SerializeField] private bool followMouse = true;
    [Tooltip("Enable debug logs for troubleshooting")]
    [SerializeField] private bool enableDebugLogs = false;
    
    private RectTransform rectTransform;
    private RectTransform backgroundRect; // Cache background RectTransform
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (background != null)
        {
            backgroundRect = background.GetComponent<RectTransform>();
            if (enableDebugLogs)
            {
                Debug.Log($"[MaskAttributeDisplay] Background found: {background.name}, RectTransform: {backgroundRect != null}");
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[MaskAttributeDisplay] Background is null!");
        }
        
        canvas = GetComponentInParent<Canvas>();
        if (enableDebugLogs)
        {
            if (canvas != null)
            {
                Debug.Log($"[MaskAttributeDisplay] Canvas found: {canvas.name}, RenderMode: {canvas.renderMode}, WorldCamera: {canvas.worldCamera?.name ?? "null"}");
            }
            else
            {
                Debug.LogWarning("[MaskAttributeDisplay] Canvas not found in parent!");
            }
        }

        if (Instance == null)
        {
            Instance = this;
            // Optional: Don't destroy on load if you want it persistent
            // DontDestroyOnLoad(gameObject);
            if (enableDebugLogs) Debug.Log("[MaskAttributeDisplay] Instance created");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Hide on start
        ClearAttributes();
    }

    private void OnEnable()
    {
        DialogueManager.OnGlobalDialogueStart += ClearAttributes;
    }

    private void OnDisable()
    {
        DialogueManager.OnGlobalDialogueStart -= ClearAttributes;
    }

    private void Update()
    {
        if (followMouse && IsTooltipVisible())
        {
            UpdatePosition();
        }
    }

    private bool IsTooltipVisible()
    {
        if (background != null) return background.activeSelf;
        if (attributeText != null) return attributeText.gameObject.activeSelf;
        return false;
    }

    private void UpdatePosition()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                if (enableDebugLogs) Debug.LogWarning("[MaskAttributeDisplay] Canvas not found!");
                return;
            }
        }

        // Determine which object to move: the Background (Image) or this script's GameObject
        RectTransform targetRect = backgroundRect != null ? backgroundRect : rectTransform;
        if (targetRect == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[MaskAttributeDisplay] Target RectTransform is null!");
            return;
        }

        // Get the root canvas (might be nested)
        Canvas rootCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
        RectTransform rootCanvasRect = rootCanvas.transform as RectTransform;
        if (rootCanvasRect == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[MaskAttributeDisplay] Root Canvas RectTransform is null!");
            return;
        }

        // Get mouse screen position
        Vector2 mouseScreenPos = Input.mousePosition;

        if (enableDebugLogs)
        {
            Debug.Log($"[MaskAttributeDisplay] Mouse: {mouseScreenPos}, Canvas Mode: {rootCanvas.renderMode}");
        }

        // Convert screen point to local point in canvas (without offset first)
        Vector2 localPoint;
        Camera cam = null;
        
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // For Overlay: no camera needed
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRect,
                mouseScreenPos,
                null,
                out localPoint);
            
            if (enableDebugLogs && !success)
            {
                Debug.LogWarning("[MaskAttributeDisplay] Failed to convert screen point to local point (Overlay mode)");
            }
        }
        else
        {
            // For Camera Space: need the camera
            cam = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
            if (cam == null)
            {
                if (enableDebugLogs) Debug.LogWarning("[MaskAttributeDisplay] Camera is null for Camera Space mode!");
                return;
            }
            
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvasRect,
                mouseScreenPos,
                cam,
                out localPoint);
            
            if (enableDebugLogs && !success)
            {
                Debug.LogWarning($"[MaskAttributeDisplay] Failed to convert screen point to local point (Camera mode, Camera: {cam.name})");
            }
        }

        // Apply offset in local space (this works correctly for both Overlay and Camera Space)
        localPoint.x += offset.x;
        localPoint.y += offset.y;

        // Convert local point to world position
        Vector3 worldPos = rootCanvasRect.TransformPoint(localPoint);

        if (enableDebugLogs)
        {
            Debug.Log($"[MaskAttributeDisplay] LocalPoint: {localPoint}, WorldPos: {worldPos}, Target: {targetRect.name}, Current Pos: {targetRect.position}");
        }

        // Set the position
        targetRect.position = worldPos;
    }

    public void ShowAttributes(string text)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MaskAttributeDisplay] ShowAttributes called with text: {text}");
        }

        if (attributeText != null)
        {
            attributeText.gameObject.SetActive(true);
            attributeText.text = text;
        }
        if (background != null)
        {
            background.SetActive(true);
            // Ensure the tooltip renders on top of everything else in its parent
            background.transform.SetAsLastSibling();
            
            if (enableDebugLogs)
            {
                Debug.Log($"[MaskAttributeDisplay] Background activated: {background.name}, Active: {background.activeSelf}, Position: {background.transform.position}");
            }
        }
        else
        {
             transform.SetAsLastSibling();
        }

        // Update position immediately so it doesn't appear in the wrong place for one frame
        UpdatePosition();
    }

    public void ClearAttributes()
    {
        if (attributeText != null)
        {
            attributeText.text = "";
            attributeText.gameObject.SetActive(false);
        }
        if (background != null)
        {
            background.SetActive(false);
        }
    }
}

