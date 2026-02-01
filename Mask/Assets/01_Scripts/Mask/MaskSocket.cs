using UnityEngine;
using UnityEngine.EventSystems;

public class MaskSocket : MonoBehaviour, IDropHandler
{
    [Header("Settings")]
    public MaskPartType acceptedType; // 这个插槽接受什么类型的碎片
    public float snapRadius = 100f;   // 吸附半径

    [Header("State")]
    public MaskPart currentPart;      // 当前吸附的碎片

    public bool IsOccupied => currentPart != null;

    private void Awake()
    {
        // 可视化调试：如果是编辑器模式，可以画个圈看看范围（用Gizmos）
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 拖拽松手时会自动调用这个（如果MaskPart在拖拽结束时没处理的话）
        // 但通常我们在 MaskPart 的 OnEndDrag 里主动寻找最近的 Socket 会更顺滑
    }

    /// <summary>
    /// 尝试吸附一个碎片
    /// </summary>
    public bool TrySnap(MaskPart part)
    {
        // 1. 检查类型是否匹配 (Decoration 可以放任意位置，或者专门的槽)
        if (part.partType != acceptedType && acceptedType != MaskPartType.Decoration)
            return false;
        
        // 2. 检查距离
        float distance = Vector3.Distance(part.transform.position, transform.position);
        // 注意：如果是UI，position是世界坐标，distance也是世界距离。
        // 如果Canvas Scale很大，100f可能很小。建议根据Canvas调整。
        
        if (distance <= snapRadius)
        {
            AttachPart(part);
            return true;
        }

        return false;
    }

    public void AttachPart(MaskPart part)
    {
        // 如果已经有东西了，把它挤走？或者禁止吸附？
        // 这里简化为：直接替换，或者把旧的弹飞。
        if (currentPart != null && currentPart != part)
        {
            currentPart.OnDetached();
        }

        currentPart = part;
        part.OnAttached(this);
    }

    public void DetachCurrent()
    {
        currentPart = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }
}

