using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapMenu : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject scene1ShadowObj;
    [SerializeField] private GameObject scene2ShadowObj;
    [SerializeField] private GameObject scene3ShadowObj;

    [Header("Scene Settings")]
    [SerializeField] private string scene1Name = "CrimeScene";
    [SerializeField] private string scene1SpawnPoint = "SpawnPoint";
    [SerializeField] private string scene2Name = "Interrogation Room";
    [SerializeField] private string scene2SpawnPoint = "SpawnPoint";
    [SerializeField] private string scene3Name = "Suspect";
    [SerializeField] private string scene3SpawnPoint = "SpawnPoint";

    private bool isMapOpen = false;

    private void Start()
    {
        if (mapPanel != null)
        {
            mapPanel.SetActive(false);
        }
        SetupMapLocation(scene1ShadowObj, scene1Name, scene1SpawnPoint);
        SetupMapLocation(scene2ShadowObj, scene2Name, scene2SpawnPoint);
        SetupMapLocation(scene3ShadowObj, scene3Name, scene3SpawnPoint);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckEventSystem();
    }
    
    private void CheckEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("Scene missing EventSystem, creating one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
    }

    private void SetupMapLocation(GameObject shadowObj, string sceneName, string spawnPointName)
    {
        if (shadowObj == null) return;

        // Ensure object is active so it can receive events
        shadowObj.SetActive(true);

        // Set initial alpha to 0 (invisible but clickable)
        SetImageAlpha(shadowObj, 0f);

        EventTrigger trigger = shadowObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = shadowObj.AddComponent<EventTrigger>();
        }

        // Click
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick; 
        clickEntry.callback.AddListener((data) => { LoadScene(sceneName, spawnPointName); });
        trigger.triggers.Add(clickEntry);

        // Pointer Enter (Hover Start -> Visible)
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { 
            SetImageAlpha(shadowObj, 1f);
        });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit (Hover End -> Invisible)
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { 
             SetImageAlpha(shadowObj, 0f);
        });
        trigger.triggers.Add(exitEntry);
    }

    private void SetImageAlpha(GameObject obj, float alpha)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    private void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        if (mapPanel != null)
        {
            mapPanel.SetActive(isMapOpen);
            if (isMapOpen)
            {
                // Reset all shadows to invisible when map is opened
                if (scene1ShadowObj != null) SetImageAlpha(scene1ShadowObj, 0f);
                if (scene2ShadowObj != null) SetImageAlpha(scene2ShadowObj, 0f);
                if (scene3ShadowObj != null) SetImageAlpha(scene3ShadowObj, 0f);
            }
        }
    }

    private void LoadScene(string sceneName, string spawnPointName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == sceneName)
        {
            Debug.Log($"Already in scene: {sceneName}");
            return;
        }
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.Log($"Loading scene: {sceneName} at {spawnPointName}");
            
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.nextSpawnPointID = spawnPointName;
            }

            if (mapPanel != null)
            {
                mapPanel.SetActive(false);
                isMapOpen = false;
            }
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' cannot be loaded. Check Build Settings!");
        }
    }
}
