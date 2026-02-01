using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTeleporter : MonoBehaviour
{
    [Header("Components to Disable/Enable")]
    [SerializeField] private SpriteRenderer playerRenderer;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private PlayerController2D playerController;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // REMOVED: Interrogation Room check logic is moved to MapMenu or handled otherwise.
        /*
        // Interrogation Room check
        if (scene.name == "Interrogation Room")
        {
            // Disable visuals and controls
            if (playerRenderer != null) playerRenderer.enabled = false;
            if (playerCollider != null) playerCollider.enabled = false;
            if (playerController != null) playerController.enabled = false;
        }
        else
        {
            // Re-enable everything
            if (playerRenderer != null) playerRenderer.enabled = true;
            if (playerCollider != null) playerCollider.enabled = true;
            if (playerController != null) playerController.enabled = true;
        }
        */

        string targetSpawnName = "SpawnPoint";

        if (GameStateManager.Instance != null && !string.IsNullOrEmpty(GameStateManager.Instance.nextSpawnPointID))
        {
            targetSpawnName = GameStateManager.Instance.nextSpawnPointID;
            GameStateManager.Instance.nextSpawnPointID = null;
        }

        GameObject spawnPoint = GameObject.Find(targetSpawnName);
        if (spawnPoint == null && targetSpawnName != "SpawnPoint")
        {
            Debug.LogWarning($"Spawn point '{targetSpawnName}' not found in {scene.name}. Trying default 'SpawnPoint'.");
            spawnPoint = GameObject.Find("SpawnPoint");
        }

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }
}
