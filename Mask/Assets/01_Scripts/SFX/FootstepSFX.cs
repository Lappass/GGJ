using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSFX : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip[] footstepClips;

    [Header("Tuning")]
    [Range(0f, 1f)] public float volume = 0.6f;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private AudioSource src;
    private int lastIndex = -1;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        if (footstepClips.Length > 1 && index == lastIndex)
            index = (index + 1) % footstepClips.Length;

        lastIndex = index;

        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        src.PlayOneShot(footstepClips[index], volume);
    }
}
