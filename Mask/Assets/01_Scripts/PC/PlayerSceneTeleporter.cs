using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneTeleporter : MonoBehaviour
{
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
