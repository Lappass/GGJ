using System;
using System.Collections.Generic;
using UnityEngine;

public class MaskAudio : MonoBehaviour
{
    public static MaskAudio Instance { get; private set; }

    [Header("Audio Source (2D UI SFX)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Generic 'Return' Clips (Size = 5)")]
    [Tooltip("Used for: 1) Return-to-backpack success, 2) Identity placed to Assemble socket")]
    [SerializeField] private AudioClip[] returnClips = new AudioClip[5];

    [Header("Emotion Clips (Used when Emotion placed to Assemble socket)")]
    [SerializeField] private List<EmotionClip> emotionClips = new List<EmotionClip>();


    [Header("Success Clip (Stage requirement met)")]
    [SerializeField] private AudioClip successClip;

    [Serializable]
    public class EmotionClip
    {
        public EmotionType emotion;
        public AudioClip clip;
    }

    public void PlaySuccess()
    {
        if (sfxSource == null) return;
        if (successClip == null) return;
        sfxSource.PlayOneShot(successClip);
    }

    private int _lastReturnIdx = -1;
    private Dictionary<EmotionType, AudioClip> _emotionMap;

    private void Awake()
    {
        Debug.Log("[MaskAudio] Awake");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        // Build lookup for emotion clips
        _emotionMap = new Dictionary<EmotionType, AudioClip>();
        foreach (var e in emotionClips)
        {
            if (e == null) continue;
            if (!_emotionMap.ContainsKey(e.emotion) && e.clip != null)
                _emotionMap.Add(e.emotion, e.clip);
        }
    }

    /// <summary>
    /// Call ONLY when a part is successfully attached to a socket.
    /// isAssembleSocket:
    ///   false = backpack socket -> random returnClips
    ///   true  = assemble socket  -> Emotion: unique clip; Identity: random returnClips
    /// </summary>
    public void PlayOnAttach(bool isAssembleSocket, MaskAttributeData data)
    {
        Debug.Log($"[MaskAudio] PlayOnAttach called. assemble={isAssembleSocket}, data={(data == null ? "NULL" : data.type.ToString())}");

        if (sfxSource == null) return;

        // 1) Backpack attach => random returnClips
        if (!isAssembleSocket)
        {
            PlayRandomReturn();
            return;
        }

        // Assemble attach:
        // 2) Emotion => unique clip
        if (data != null && data.type == AttributeType.Emotion)
        {
            var emoClip = GetEmotionClip(data.emotionValue);
            if (emoClip != null)
            {
                sfxSource.PlayOneShot(emoClip);
                return;
            }

            // Fallback (if you forgot to assign an emotion clip)
            PlayRandomReturn();
            return;
        }

        // 3) Identity (or null data) => random returnClips
        PlayRandomReturn();
    }

    private AudioClip GetEmotionClip(EmotionType emo)
    {
        if (_emotionMap == null) return null;
        return _emotionMap.TryGetValue(emo, out var clip) ? clip : null;
    }

    private void PlayRandomReturn()
    {
        if (returnClips == null || returnClips.Length == 0) return;

        // collect valid clips
        int validCount = 0;
        for (int i = 0; i < returnClips.Length; i++)
            if (returnClips[i] != null) validCount++;

        if (validCount == 0) return;

        // if only one valid, just play it
        if (validCount == 1)
        {
            for (int i = 0; i < returnClips.Length; i++)
            {
                if (returnClips[i] != null)
                {
                    sfxSource.PlayOneShot(returnClips[i]);
                    _lastReturnIdx = i;
                    return;
                }
            }
        }

        // pick random index (avoid immediate repeat)
        int idx = UnityEngine.Random.Range(0, returnClips.Length);
        int guard = 0;
        while ((returnClips[idx] == null || idx == _lastReturnIdx) && guard < 20)
        {
            idx = UnityEngine.Random.Range(0, returnClips.Length);
            guard++;
        }

        if (returnClips[idx] != null)
        {
            _lastReturnIdx = idx;
            sfxSource.PlayOneShot(returnClips[idx]);
        }
        else
        {
            // fallback: find first valid
            for (int i = 0; i < returnClips.Length; i++)
            {
                if (returnClips[i] != null)
                {
                    _lastReturnIdx = i;
                    sfxSource.PlayOneShot(returnClips[i]);
                    return;
                }
            }
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log("[MaskAudio] Update running (P pressed)");
    }
}
