using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MaskAssembler : MonoBehaviour
{
    public static MaskAssembler Instance { get; private set; }

    [Header("Configuration")]
    public List<MaskSocket> requiredSockets; // 必须填满的插槽（例如眼睛嘴巴）

    [Header("Events")]
    public UnityEvent OnAssemblyComplete;
    public UnityEvent OnAssemblyIncomplete;

    private void Awake()
    {
        Instance = this;
    }

    public void CheckAssembly()
    {
        bool complete = true;
        foreach (var socket in requiredSockets)
        {
            if (!socket.IsOccupied)
            {
                complete = false;
                break;
            }
        }

        if (complete)
        {
            Debug.Log("Mask Assembled!");
            OnAssemblyComplete?.Invoke();
        }
        else
        {
            OnAssemblyIncomplete?.Invoke();
        }
    }
}



