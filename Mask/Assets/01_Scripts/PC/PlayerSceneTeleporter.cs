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
        // 寻找当前场景中的出生点
        // 约定：每个场景里必须有一个叫 "SpawnPoint" 的物体
        GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (spawnPoint != null)
        {
            // 移动玩家到出生点位置
            transform.position = spawnPoint.transform.position;
            Debug.Log($"玩家已移动到场景 {scene.name} 的出生点: {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning($"在场景 {scene.name} 中没有找到 'SpawnPoint' 物体，玩家位置未重置！");
        }
    }
}

