using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapMenu : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject scene1ImageObj;
    [SerializeField] private GameObject scene2ImageObj;
    [SerializeField] private GameObject scene3ImageObj;

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
        AddClickListener(scene1ImageObj, scene1Name, scene1SpawnPoint);
        AddClickListener(scene2ImageObj, scene2Name, scene2SpawnPoint);
        AddClickListener(scene3ImageObj, scene3Name, scene3SpawnPoint);
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
    private void AddClickListener(GameObject obj, string sceneName, string spawnPointName)
    {
        if (obj == null) return;
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick; 
        entry.callback.AddListener((data) => { LoadScene(sceneName, spawnPointName); });

        trigger.triggers.Add(entry);
    }

    private void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        if (mapPanel != null)
        {
            mapPanel.SetActive(isMapOpen);
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
