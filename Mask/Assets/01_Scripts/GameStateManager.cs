using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    //a dictionary to keep track of states
    private Dictionary<string, bool> itemStates = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetState(string id, bool state)
    {
        if (itemStates.ContainsKey(id))
        {
            itemStates[id] = state;
        }
        else
        {
            itemStates.Add(id, state);
        }
    }
    public bool GetState(string id)
    {
        if (itemStates.ContainsKey(id))
        {
            return itemStates[id];
        }
        return false;
    }
}

