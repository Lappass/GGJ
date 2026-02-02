using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] // 确保有Image组件用于射线检测
public class MaskPart : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Data")]
    public MaskPartType partType;

    [Header("Settings")]
    public bool returnToStartPosOnFail = true;
    public MaskAttributeData attributeData;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 startDragPos;
    private MaskSocket currentSocket;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // 开启 Alpha Hit Test (如果是 L 形图片，必须开启 Read/Write Enabled 在贴图设置里)
        // 这样点击透明区域就不会拿起来
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSocket != null)
        {
            currentSocket.DetachCurrent();
            currentSocket = null;
        }

        startDragPos = transform.position;
        originalParent = transform.parent;
        
        // 提到最上层显示
        transform.SetParent(transform.root); 
        transform.SetAsLastSibling();
        
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        
        // 寻找最近的 Socket
        MaskSocket bestSocket = FindBestSocket();

        if (bestSocket != null)
        {
            // 吸附成功
            bestSocket.AttachPart(this);
        }
        else
        {
            // 吸附失败
            if (returnToStartPosOnFail)
            {
                transform.position = startDragPos;
                transform.SetParent(originalParent); // 回到原来的层级/背包
            }
        }

        // 通知管理器检查状态（可选）
        MaskAssembler.Instance?.CheckAssembly();
    }

    public void OnAttached(MaskSocket socket)
    {
        currentSocket = socket;
        transform.SetParent(socket.transform);
        transform.localPosition = Vector3.zero; // 归位到 Socket 中心
    }

    public void OnDetached()
    {
        currentSocket = null;
        // 逻辑上可能需要回到“背包”或者悬空，这里暂时不动
    }

    private MaskSocket FindBestSocket()
    {
        // 这种查找方式效率较低，如果 Socket 很多建议用管理器统一维护列表
        MaskSocket[] sockets = FindObjectsByType<MaskSocket>(FindObjectsSortMode.None);
        
        MaskSocket best = null;
        float minDst = float.MaxValue;

        foreach (var socket in sockets)
        {
            if (socket.TrySnap(this)) // TrySnap 里面包含了距离和类型判断
            {
                // 这里 TrySnap 只是试探，并不真的吸附，我们需要稍微改一下 Socket 的逻辑
                // 或者我们在这里手动算距离
                
                // 重新修正逻辑：只在这里算距离和类型
                if (socket.acceptedType != partType && socket.acceptedType != MaskPartType.Decoration) continue;

                float dst = Vector3.Distance(transform.position, socket.transform.position);
                if (dst <= socket.snapRadius && dst < minDst)
                {
                    minDst = dst;
                    best = socket;
                }
            }
        }
        return best;
    }
}




