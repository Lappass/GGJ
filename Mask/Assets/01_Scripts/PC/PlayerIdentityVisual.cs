using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentityVisual : MonoBehaviour
{
    [Serializable]
    public class IdentityOverride
    {
        public IdentityType identity;
        public AnimatorOverrideController overrideController;
    }

    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Header("Overrides")]
    [SerializeField] private AnimatorOverrideController defaultOverride; // 没身份/未匹配时用
    [SerializeField] private List<IdentityOverride> overrides = new List<IdentityOverride>();

    private IdentityType _currentIdentity;
    private Dictionary<IdentityType, AnimatorOverrideController> _map;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        _map = new Dictionary<IdentityType, AnimatorOverrideController>();
        foreach (var it in overrides)
        {
            if (it != null && it.overrideController != null)
                _map[it.identity] = it.overrideController;
        }
    }

    private void OnEnable()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.OnMaskStateChanged += HandleMaskChanged;

            HandleMaskChanged(MaskManager.Instance.CurrentIdentity, MaskManager.Instance.CurrentEmotions);
        }
    }

    private void OnDisable()
    {
        if (MaskManager.Instance != null)
            MaskManager.Instance.OnMaskStateChanged -= HandleMaskChanged;
    }

    private void HandleMaskChanged(IdentityType identity, IReadOnlyList<EmotionType> emotions)
    {
        if (animator == null) return;
        if (identity.Equals(_currentIdentity)) return;

        _currentIdentity = identity;

        var target = defaultOverride;
        if (_map != null && _map.TryGetValue(identity, out var oc) && oc != null)
            target = oc;

        if (target == null) return;

        int stateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

        animator.runtimeAnimatorController = target;

        animator.Play(stateHash, 0, normalizedTime);
        animator.Update(0f);
    }
}
