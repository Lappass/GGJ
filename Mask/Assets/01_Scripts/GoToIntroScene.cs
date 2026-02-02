using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToIntroScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the intro scene to load")]
    [SerializeField] private string introSceneName = "Intro";

    [Header("Input Settings")]
    [Tooltip("Which mouse button to use (0 = Left, 1 = Right, 2 = Middle)")]
    [SerializeField] private int mouseButton = 0;
    [Tooltip("Or use a keyboard key instead")]
    [SerializeField] private KeyCode keyboardKey = KeyCode.None;

    [Header("Options")]
    [Tooltip("Click anywhere on screen to go to intro scene")]
    [SerializeField] private bool clickAnywhere = true;
    [Tooltip("Require clicking on this GameObject (requires Collider2D)")]
    [SerializeField] private bool requireClickOnObject = false;

    private void Update()
    {
        bool shouldLoad = false;

        // Check mouse click
        if (mouseButton >= 0 && mouseButton <= 2)
        {
            if (Input.GetMouseButtonDown(mouseButton))
            {
                if (clickAnywhere)
                {
                    shouldLoad = true;
                }
                else if (requireClickOnObject)
                {
                    // Check if click hit this object
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mousePos.z = 0f;
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null && col.OverlapPoint(mousePos))
                    {
                        shouldLoad = true;
                    }
                }
            }
        }

        // Check keyboard key
        if (keyboardKey != KeyCode.None && Input.GetKeyDown(keyboardKey))
        {
            shouldLoad = true;
        }

        if (shouldLoad)
        {
            LoadIntroScene();
        }
    }

    private void LoadIntroScene()
    {
        if (string.IsNullOrEmpty(introSceneName))
        {
            Debug.LogError("GoToIntroScene: Intro scene name is empty!");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(introSceneName))
        {
            Debug.Log($"Loading intro scene: {introSceneName}");
            
            // Use SceneTransition if available for smooth fade effect
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.TransitionToScene(introSceneName, "SpawnPoint");
            }
            else
            {
                // Fallback: direct load
                SceneManager.LoadScene(introSceneName);
            }
        }
        else
        {
            Debug.LogError($"GoToIntroScene: Scene '{introSceneName}' cannot be loaded. Check Build Settings!");
        }
    }
}

