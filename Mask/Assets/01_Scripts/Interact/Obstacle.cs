using System.Collections;
using UnityEngine;

public class Obstacle : Interactable
{
    [Header("Settings")]
    [Tooltip("Unique ID for saving state across scenes")]
    [SerializeField] private string objectID = "Obstacle_01";

    [Header("Priority")]
    [SerializeField] private int priority = 1000;
    public override int Priority => priority;

    [Header("Move Target (required)")]
    public Transform targetPoint;

    [SerializeField] private float moveDuration = 0.35f; 
    [SerializeField] private Collider2D triggerColliderToDisable; 

    private bool _used = false;
    private bool _moving = false;

    public override bool CanInteract => !_used && !_moving && targetPoint != null;

    private void Start()
    {
        if (GameStateManager.Instance != null && targetPoint != null)
        {
            _used = GameStateManager.Instance.GetState(objectID);
            if (_used)
            {
                // Already moved, snap to target directly
                transform.position = targetPoint.position;
                if (triggerColliderToDisable != null)
                    triggerColliderToDisable.enabled = false;
            }
        }
    }

    public override void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract) return;

        _used = true;
        _moving = true;

        if (triggerColliderToDisable != null)
            triggerColliderToDisable.enabled = false;

        // Save state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(objectID, true);
        }

        StartCoroutine(MoveToTargetRoutine(targetPoint.position));
    }

    private IEnumerator MoveToTargetRoutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;

        if (moveDuration <= 0.0001f)
        {
            transform.position = targetPos;
            _moving = false;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.Clamp01(t));
            yield return null;
        }

        transform.position = targetPos;
        _moving = false;
    }
}
