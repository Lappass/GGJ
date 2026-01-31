using UnityEngine;

public class InteractPrompt : MonoBehaviour
{
    [Header("Drag your prompt Canvas (Player/Canvas) here")]
    [SerializeField] private GameObject promptRoot;

    // 可选：如果你不想“所有trigger都提示”（比如 CameraBounds 这种trigger也会进来）
    // 建议你做一个 Interactable 层，只有可交互触发器用这个层
    [Header("Optional filter")]
    [SerializeField] private LayerMask interactableLayers = ~0; // 默认所有层都算
    [SerializeField] private string requiredTag = "";           // 例如 "Interactable"，不需要就留空

    private int _insideCount = 0;

    private void Awake()
    {
        if (promptRoot != null) promptRoot.SetActive(false); // 开局隐藏
    }

    private bool PassFilter(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & interactableLayers) == 0) return false;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return false;
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!PassFilter(other)) return;

        _insideCount++;
        if (promptRoot != null) promptRoot.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!PassFilter(other)) return;

        _insideCount = Mathf.Max(0, _insideCount - 1);
        if (_insideCount == 0 && promptRoot != null) promptRoot.SetActive(false);
    }
}
