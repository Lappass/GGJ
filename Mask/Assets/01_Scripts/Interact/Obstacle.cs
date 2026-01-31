using System.Collections;
using UnityEngine;

public class Obstacle : Interactable
{
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

    public override void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract) return;

        _used = true;
        _moving = true;

        if (triggerColliderToDisable != null)
            triggerColliderToDisable.enabled = false;

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
