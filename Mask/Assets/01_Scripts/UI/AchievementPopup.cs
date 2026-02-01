using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementPopup : MonoBehaviour
{
    public static AchievementPopup Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private UnlockableDatabase database;

    [Header("UI References")]
    [Tooltip("If the script is ON this object, leave this empty or ensure it has a CanvasGroup.")]
    [SerializeField] private GameObject popupPanel; 
    [SerializeField] private Image iconImage;       // Icon for Identity/Emotion
    [SerializeField] private TextMeshProUGUI titleText; // "New Identity Unlocked!"
    [SerializeField] private TextMeshProUGUI descriptionText; // "Detective" / "Happy"
    
    private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private Vector2 offScreenPos = new Vector2(300, -300); // Adjust based on your UI layout
    [SerializeField] private Vector2 onScreenPos = new Vector2(-20, 20); // Bottom Right relative to anchor

    private struct PopupData
    {
        public string title;
        public string description;
        public Sprite icon;
    }

    private Queue<PopupData> popupQueue = new Queue<PopupData>();
    private bool isShowing = false;
    private RectTransform panelRect;

    // Track shown IDs during this session only (no PlayerPrefs)
    private HashSet<string> sessionShownIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (popupPanel != null)
        {
            panelRect = popupPanel.GetComponent<RectTransform>();
            
            // Auto-setup CanvasGroup for visibility
            canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupPanel.AddComponent<CanvasGroup>();
            }
            
            // Hide initially using Alpha/Raycast, NOT SetActive(false)
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void CheckAndShowUnlock(AttributeType type, int value)
    {
        if (database == null) 
        {
            Debug.LogWarning("[AchievementPopup] Database is missing! Please assign UnlockableDatabase in Inspector.");
            return;
        }

        UnlockableDatabase.UnlockEntry entry = null;
        if (type == AttributeType.Identity)
            entry = database.GetEntry((IdentityType)value);
        else if (type == AttributeType.Emotion)
            entry = database.GetEntry((EmotionType)value);

        if (entry == null) 
        {
             Debug.LogWarning($"[AchievementPopup] Entry not found in database for Type: {type}, Value: {value}. Please check UnlockableDatabase configuration.");
             return;
        }

        // Check if already shown in this session
        if (sessionShownIds.Contains(entry.id)) 
        {
            Debug.Log($"[AchievementPopup] Already shown this session: {entry.id}");
            return;
        }

        // Mark as shown in this session
        sessionShownIds.Add(entry.id);

        Debug.Log($"[AchievementPopup] Showing unlock for: {entry.displayName}");
        // Show
        ShowUnlock("Unlocked New Mask", entry.displayName, entry.icon);
    }

    public void ShowUnlock(string title, string itemName, Sprite icon = null)
    {
        popupQueue.Enqueue(new PopupData 
        { 
            title = title, 
            description = itemName, 
            icon = icon 
        });

        if (!isShowing)
        {
            StartCoroutine(DisplayQueueRoutine());
        }
    }

    private IEnumerator DisplayQueueRoutine()
    {
        isShowing = true;

        while (popupQueue.Count > 0)
        {
            PopupData data = popupQueue.Dequeue();
            SetupPopup(data);

            // Animate In
            yield return StartCoroutine(SlideIn());

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Animate Out
            yield return StartCoroutine(SlideOut());

            // Small buffer between popups
            yield return new WaitForSeconds(0.2f);
        }

        isShowing = false;
    }

    private void SetupPopup(PopupData data)
    {
        // Don't SetActive(true), just prepare data. Visibility handled by alpha in SlideIn.
        if (titleText != null) titleText.text = data.title;
        if (descriptionText != null) descriptionText.text = data.description;
        
        if (iconImage != null)
        {
            if (data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator SlideIn()
    {
        if (panelRect == null) yield break;

        float timer = 0f;
        panelRect.anchoredPosition = offScreenPos;
        
        // Ensure visible
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float t = timer / slideDuration;
            
            // Fade In
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(t * 2);

            // Ease Out Back
            float curve = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2); 
            panelRect.anchoredPosition = Vector2.LerpUnclamped(offScreenPos, onScreenPos, curve);
            yield return null;
        }
        panelRect.anchoredPosition = onScreenPos;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private IEnumerator SlideOut()
    {
        if (panelRect == null) yield break;

        float timer = 0f;
        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            float t = timer / slideDuration;
            
            // Fade Out
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(1 - t * 2);

            // Ease In
            float curve = t * t;
            panelRect.anchoredPosition = Vector2.Lerp(onScreenPos, offScreenPos, curve);
            yield return null;
        }
        panelRect.anchoredPosition = offScreenPos;
        
        // Fully hide
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // Helper to test
    [ContextMenu("Test Popup")]
    public void TestPopup()
    {
        ShowUnlock("New Identity", "Detective");
    }
}
