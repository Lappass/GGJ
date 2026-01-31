using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnPointName = "SpawnPoint";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController2D>() != null)
        {
            LoadTargetScene();
        }
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneTransitionTrigger: Target scene name is empty!");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.nextSpawnPointID = targetSpawnPointName;
            }

            Debug.Log($"Switching to scene '{targetSceneName}' at spawn point '{targetSpawnPointName}'");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError($"SceneTransitionTrigger: Cannot load scene '{targetSceneName}'. Check Build Settings.");
        }
    }
}
