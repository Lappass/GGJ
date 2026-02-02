using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

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

    [Header("Teleport Settings")]
    [Tooltip("The root object to teleport (e.g., EssentialSystem). If null, will auto-find from player.")]
    [SerializeField] private Transform rootToTeleport;
    [Tooltip("Delay before teleporting to spawn point (to ensure scene is fully loaded)")]
    [SerializeField] private float teleportDelay = 0.1f;

    private bool isMapOpen = false;
    private bool _canToggle = true;

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
    
    private void OnEnable()
    {
        DialogueManager.OnGlobalDialogueStart += OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd += OnDialogueEnd;
    }

    private void OnDisable()
    {
        DialogueManager.OnGlobalDialogueStart -= OnDialogueStart;
        DialogueManager.OnGlobalDialogueEnd -= OnDialogueEnd;
    }

    private void OnDialogueStart() => _canToggle = false;
    private void OnDialogueEnd() => _canToggle = true;

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckEventSystem();
        StartCoroutine(TeleportToSpawnPoint(scene));
    }

    private Transform FindRootParent(Transform startTransform)
    {
        // Find the root parent (EssentialSystem or DontDestroyOnLoad object)
        Transform current = startTransform;
        Transform root = current;
        
        while (current.parent != null)
        {
            current = current.parent;
            root = current;
        }
        
        return root;
    }

    private IEnumerator TeleportToSpawnPoint(Scene scene)
    {
        // Wait a bit to ensure scene objects are fully initialized
        yield return new WaitForSeconds(teleportDelay);

        // Find root object if not assigned
        Transform root = rootToTeleport;
        if (root == null && playerController != null)
        {
            root = FindRootParent(playerController.transform);
        }
        else if (root == null && playerRenderer != null)
        {
            root = FindRootParent(playerRenderer.transform);
        }

        if (root == null)
        {
            Debug.LogWarning("MapMenu: Could not find root object to teleport. Player position unchanged.");
            yield break;
        }

        string targetSpawnName = "SpawnPoint";

        if (GameStateManager.Instance != null && !string.IsNullOrEmpty(GameStateManager.Instance.nextSpawnPointID))
        {
            targetSpawnName = GameStateManager.Instance.nextSpawnPointID;
            GameStateManager.Instance.nextSpawnPointID = null;
        }

        // Try to find spawn point
        GameObject spawnPoint = GameObject.Find(targetSpawnName);
        if (spawnPoint == null && targetSpawnName != "SpawnPoint")
        {
            Debug.LogWarning($"Spawn point '{targetSpawnName}' not found in {scene.name}. Trying default 'SpawnPoint'.");
            spawnPoint = GameObject.Find("SpawnPoint");
        }

        if (spawnPoint != null)
        {
            // Teleport the root object (EssentialSystem) to the spawn point
            root.position = spawnPoint.transform.position;
            Debug.Log($"Root object '{root.name}' teleported to spawn point: {targetSpawnName} at position {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogError($"No spawn point found in scene '{scene.name}'. Root position unchanged.");
        }
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
        if (!_canToggle) return;

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

            if (mapPanel != null)
            {
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

            // Use SceneTransition for smooth fade effect
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.TransitionToScene(sceneName, spawnPointName);
            }
            else
            {
                // Fallback: direct load if transition system not available
                Debug.LogWarning("SceneTransition.Instance is null, loading scene directly.");
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.nextSpawnPointID = spawnPointName;
                }
                SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' cannot be loaded. Check Build Settings!");
        }
    }
}
