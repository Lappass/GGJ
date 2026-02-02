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

    [Header("Player References")]
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Collider2D playerCollider;

    [Header("Map SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip mapOpenSfx;
    [SerializeField] private AudioClip mapCloseSfx;

    // 切场景时是否确保播放“关地图”音效（按你需求默认 true）
    [SerializeField] private bool playCloseSfxOnSceneSwitch = true;

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
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
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

        // Clear existing triggers to avoid duplicates if Start calls multiple times
        EventTrigger trigger = shadowObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = shadowObj.AddComponent<EventTrigger>();
        }
        else
        {
            trigger.triggers.Clear();
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

            if (sfxSource != null)
            {
                if (isMapOpen && mapOpenSfx != null)
                    sfxSource.PlayOneShot(mapOpenSfx);
                else if (!isMapOpen && mapCloseSfx != null)
                    sfxSource.PlayOneShot(mapCloseSfx);
            }

            if (isMapOpen)
            {
                // Reset all shadows to invisible when map is opened
                if (scene1ShadowObj != null) SetImageAlpha(scene1ShadowObj, 0f);
                if (scene2ShadowObj != null) SetImageAlpha(scene2ShadowObj, 0f);
                if (scene3ShadowObj != null) SetImageAlpha(scene3ShadowObj, 0f);
            }
        }
    }
    private void PlaySfxPersistAcrossSceneLoad(AudioClip clip)
    {
        if (clip == null) return;

        GameObject go = new GameObject("Temp_MapSFX");
        DontDestroyOnLoad(go);

        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 0f;      // 2D
        src.playOnAwake = false;
        src.loop = false;
        src.clip = clip;
        src.Play();

        Destroy(go, clip.length + 0.1f);
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
                if (playCloseSfxOnSceneSwitch && isMapOpen && mapCloseSfx != null)
                {
                    PlaySfxPersistAcrossSceneLoad(mapCloseSfx);
                }

                mapPanel.SetActive(false);
                isMapOpen = false;
            }

            // --- Disable Player if going to Interrogation Room ---
            if (sceneName == "Interrogation Room")
            {
                // Disable Visuals
                if (playerRenderer != null) playerRenderer.enabled = false;

                // Disable Collider
                if (playerCollider != null) playerCollider.enabled = false;

                // Disable Control
                if (playerController != null) playerController.enabled = false;
            }
            else
            {
                // Re-enable in case we are coming from Interrogation Room
                if (playerRenderer != null) playerRenderer.enabled = true;
                if (playerCollider != null) playerCollider.enabled = true;
                if (playerController != null) playerController.enabled = true;
            }
            // ----------------------------------------------------

            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' cannot be loaded. Check Build Settings!");
        }
    }
}
