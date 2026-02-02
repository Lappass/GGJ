using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Should the black screen automatically fade out when the game starts?")]
    [SerializeField] private bool autoFadeOutOnStart = true;

    private Canvas canvas;
    private bool isTransitioning = false;
    private static bool hasInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 9999; // Ensure it's on top
                }
            }

            // On first initialization, set black screen to fully opaque for opening fade-out
            if (!hasInitialized && blackScreen != null && autoFadeOutOnStart)
            {
                blackScreen.gameObject.SetActive(true);
                Color c = blackScreen.color;
                c.a = 1f; // Start fully black
                blackScreen.color = c;
                hasInitialized = true;
            }
            else if (blackScreen != null)
            {
                // Subsequent scene loads: start transparent
                Color c = blackScreen.color;
                c.a = 0f;
                blackScreen.color = c;
                blackScreen.gameObject.SetActive(false);
            }
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Auto-fade out on first start (opening curtain effect)
        if (Instance == this && !isTransitioning && blackScreen != null && 
            blackScreen.gameObject.activeSelf && blackScreen.color.a >= 0.99f && autoFadeOutOnStart)
        {
            StartCoroutine(OpeningFadeOut());
        }
    }

    private IEnumerator OpeningFadeOut()
    {
        // Small delay to ensure everything is initialized
        yield return new WaitForSeconds(0.1f);
        
        // Fade out (curtain lifts up)
        yield return StartCoroutine(FadeOut());
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Start a scene transition with fade in/out effect
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="spawnPointName">Name of the spawn point in the new scene</param>
    public void TransitionToScene(string sceneName, string spawnPointName)
    {
        if (isTransitioning) return;
        
        StartCoroutine(TransitionCoroutine(sceneName, spawnPointName));
    }

    private IEnumerator TransitionCoroutine(string sceneName, string spawnPointName)
    {
        isTransitioning = true;

        // Step 1: Fade in (black screen falls down / appears)
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            yield return StartCoroutine(FadeIn());
        }

        // Step 2: Load the new scene
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.nextSpawnPointID = spawnPointName;
        }

        SceneManager.LoadScene(sceneName);

        // Wait for scene to load
        yield return null; // Wait one frame for scene to start loading
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == sceneName);

        // Wait a bit more to ensure everything is initialized
        yield return new WaitForSeconds(0.1f);

        // Step 3: Fade out (black screen lifts up / disappears)
        if (blackScreen != null)
        {
            yield return StartCoroutine(FadeOut());
            blackScreen.gameObject.SetActive(false);
        }

        isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = blackScreen.color;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = fadeCurve.Evaluate(t);
            c.a = curveValue;
            blackScreen.color = c;
            yield return null;
        }
        
        // Ensure fully opaque
        c.a = 1f;
        blackScreen.color = c;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color c = blackScreen.color;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / transitionDuration;
            float curveValue = fadeCurve.Evaluate(t);
            c.a = 1f - curveValue; // Reverse the curve
            blackScreen.color = c;
            yield return null;
        }
        
        // Ensure fully transparent
        c.a = 0f;
        blackScreen.color = c;
    }

    /// <summary>
    /// Manually trigger a fade in (for testing or other uses)
    /// </summary>
    public void FadeIn(float duration = -1)
    {
        if (duration > 0) transitionDuration = duration;
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Manually trigger a fade out (for testing or other uses)
    /// </summary>
    public void FadeOut(float duration = -1)
    {
        if (duration > 0) transitionDuration = duration;
        if (blackScreen != null)
        {
            StartCoroutine(FadeOut());
        }
    }
}

