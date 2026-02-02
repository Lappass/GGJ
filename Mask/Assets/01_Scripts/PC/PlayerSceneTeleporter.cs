using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerSceneTeleporter : MonoBehaviour
{
    [Header("Components to Disable/Enable")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private PlayerController2D playerController;

    [Header("Teleport Settings")]
    [Tooltip("Delay before teleporting to spawn point (to ensure scene is fully loaded)")]
    [SerializeField] private float teleportDelay = 0.1f;
    [Tooltip("The root object to teleport (e.g., EssentialSystem). If null, will find root parent with DontDestroyOnLoad.")]
    [SerializeField] private Transform rootToTeleport;

    private void OnEnable()
    {
        // 订阅场景加载完成事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        // Auto-find root if not assigned
        if (rootToTeleport == null)
        {
            rootToTeleport = FindRootParent();
        }
    }

    private Transform FindRootParent()
    {
        // Find the root parent (EssentialSystem or DontDestroyOnLoad object)
        Transform current = transform;
        Transform root = current;
        
        while (current.parent != null)
        {
            current = current.parent;
            root = current;
        }
        
        return root;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Start coroutine to teleport after a short delay
        StartCoroutine(TeleportToSpawnPoint(scene));
    }

    private IEnumerator TeleportToSpawnPoint(Scene scene)
    {
        // Wait a bit to ensure scene objects are fully initialized
        yield return new WaitForSeconds(teleportDelay);

        // Ensure we have a root to teleport
        if (rootToTeleport == null)
        {
            rootToTeleport = FindRootParent();
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

        if (spawnPoint != null && rootToTeleport != null)
        {
            // Teleport the root object (EssentialSystem) to the spawn point
            rootToTeleport.position = spawnPoint.transform.position;
            Debug.Log($"Root object '{rootToTeleport.name}' teleported to spawn point: {targetSpawnName} at position {spawnPoint.transform.position}");
        }
        else
        {
            if (spawnPoint == null)
            {
                Debug.LogError($"No spawn point found in scene '{scene.name}'. Root position unchanged.");
            }
            if (rootToTeleport == null)
            {
                Debug.LogError($"Root object to teleport is null. Cannot teleport player.");
            }
        }
    }
}
