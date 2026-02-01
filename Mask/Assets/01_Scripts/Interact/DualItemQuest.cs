using UnityEngine;
using System.Collections.Generic;

public class DualItemQuest : MonoBehaviour
{
    [Header("Target Items")]
    [Tooltip("IDs of the items to check status for. Must match objectID in PickUpItem/PickableItem.")]
    [SerializeField] private List<string> itemIDs; 

    [Header("Dialogue")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueSequence completionDialogue;

    [Header("Behavior")]
    [Tooltip("If true, waits for any currently playing dialogue to finish before starting quest dialogue.")]
    [SerializeField] private bool waitForCurrentDialogue = true;
    [Tooltip("Delay in seconds before checking/playing quest dialogue. Useful to let item pickup dialogue start first.")]
    [SerializeField] private float startDelay = 0.5f;

    [Header("Reward")]
    [Tooltip("List of fragment prefabs to unlock.")]
    [SerializeField] private List<GameObject> fragmentRewards;
    // Legacy single reward support (optional, can be removed if you migrate existing data)
    [SerializeField] private GameObject fragmentReward;
    
    [Header("Quest ID")]
    [SerializeField] private string questID = "Quest_DualItem";
    
    private bool _rewarded = false;
    private int _localCount = 0; // Fallback for no GameStateManager

    private void Start()
    {
        if (itemIDs == null || itemIDs.Count == 0)
        {
            Debug.LogWarning("DualItemQuest: No Item IDs assigned! Quest will complete immediately if triggered.");
        }

        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
            
        CheckProgress(false);
    }

    private void Update()
    {
        // Debug helper to check state in runtime inspector
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F8)) 
        {
             Debug.Log($"[DualItemQuest] Checking: {itemIDs.Count} items.");
             CheckProgress(true);
        }
    }

    // Call this method from the item's OnPickedUp event
    public void OnItemInvestigated()
    {
        Debug.Log("[DualItemQuest] OnItemInvestigated called. Checking progress...");
        _localCount++;
        CheckProgress(true);
    }

    private void CheckProgress(bool allowTrigger)
    {
        Debug.Log($"[DualItemQuest] Checking progress. allowTrigger: {allowTrigger}, rewarded: {_rewarded}");
        if (_rewarded) return;
        
        if (GameStateManager.Instance != null)
        {
             // Check if quest itself is already done
             if (GameStateManager.Instance.GetState(questID))
             {
                 _rewarded = true;
                 return;
             }

             // Count how many items are picked
             int count = 0;
             foreach (var id in itemIDs)
             {
                 bool picked = GameStateManager.Instance.GetState(id);
                 if (picked)
                 {
                     count++;
                 }
                 Debug.Log($"[DualItemQuest] Item ID '{id}' is picked: {picked}");
             }
             
             Debug.Log($"[DualItemQuest] Total Picked: {count}/{itemIDs.Count}");

             if (count >= itemIDs.Count)
             {
                 // If allowTrigger is true (called from runtime event), we play logic.
                 // If allowTrigger is false (Start), we might just mark as done or ignore?
                 // If we load the game and items are picked but quest not done, maybe we should trigger it?
                 // Let's trigger it if allowTrigger is true OR if we want to auto-complete on load.
                 // Usually auto-complete on load without dialogue is better, or play dialogue?
                 // For now, only trigger if allowTrigger is true, OR if we decide to handle load cases.
                 // If user quits mid-dialogue, they might miss reward.
                 // So if loaded and conditions met but not rewarded, we should probably just give reward silently?
                 if (allowTrigger)
                 {
                     CompleteQuest();
                 }
                 else
                 {
                     // On Start: if conditions met but not rewarded, likely missed reward or bug.
                     // Let's give reward silently or just mark true?
                     // Let's do nothing and wait for event? No, event won't fire if items gone.
                     // So we must handle it.
                     // Let's just log it and maybe give reward silently.
                     Debug.Log("Quest requirements met on load. Completing silently.");
                     GiveReward(); 
                 }
             }
        }
        else
        {
            // Testing without GameState
            if (_localCount >= itemIDs.Count && allowTrigger)
            {
                CompleteQuest();
            }
        }
    }

    private void CompleteQuest()
    {
        Debug.Log($"[DualItemQuest] CompleteQuest triggered. DialogueManager: {dialogueManager}, Sequence: {completionDialogue}");
        
        if (completionDialogue == null || dialogueManager == null)
        {
            Debug.LogWarning("[DualItemQuest] Missing DialogueManager or CompletionDialogue! Completing silently.");
            OnDialogueFinished();
            return;
        }

        StartCoroutine(PlaySequenceProcess());
    }

    private System.Collections.IEnumerator PlaySequenceProcess()
    {
        // 1. Initial Delay (allows PickUpItem's dialogue to start if any)
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);
        else
            yield return null; // Wait at least one frame

        // 2. Wait for existing dialogue to finish?
        if (waitForCurrentDialogue && dialogueManager.IsPlaying)
        {
            Debug.Log("[DualItemQuest] Waiting for current dialogue to finish...");
            bool finished = false;
            UnityEngine.Events.UnityAction onFinish = () => finished = true;
            
            // Listen once
            dialogueManager.onDialogueFinished.AddListener(onFinish);

            // Wait
            yield return new WaitUntil(() => finished);

            dialogueManager.onDialogueFinished.RemoveListener(onFinish);
            Debug.Log("[DualItemQuest] Current dialogue finished. Starting quest dialogue.");
        }

        // 3. Play Quest Dialogue
        dialogueManager.Play(completionDialogue, OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        GiveReward();
    }
    
    private void GiveReward()
    {
        if (_rewarded) return;
        _rewarded = true;

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(questID, true);
        }

        if (PlayerMaskInventoryController.Instance != null)
        {
            // Give single (legacy) reward if assigned
            if (fragmentReward != null)
            {
                PlayerMaskInventoryController.Instance.UnlockFragment(fragmentReward);
                Debug.Log("Quest Reward Given (Single): " + fragmentReward.name);
            }

            // Give list rewards
            if (fragmentRewards != null)
            {
                foreach (var frag in fragmentRewards)
                {
                    if (frag != null)
                    {
                        PlayerMaskInventoryController.Instance.UnlockFragment(frag);
                        Debug.Log("Quest Reward Given (List): " + frag.name);
                    }
                }
            }
        }
    }
}

