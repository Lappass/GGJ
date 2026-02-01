using UnityEngine;

public class WindowSFX : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private AudioSource sfxA;
    [SerializeField] private AudioSource sfxB;
    [SerializeField] private AudioClip clipA;
    [SerializeField] private AudioClip clipB;

    [Header("Match animation length")]
    [SerializeField] private bool loopUntilEnd = true;

    // Animation Event: call at the first frame of Opening
    public void SFX_OpenStart()
    {
        if (sfxA == null || sfxB == null) return;

        // A
        if (clipA != null)
        {
            sfxA.clip = clipA;
            sfxA.loop = loopUntilEnd;
            sfxA.Play();
        }

        // B
        if (clipB != null)
        {
            sfxB.clip = clipB;
            sfxB.loop = loopUntilEnd;
            sfxB.Play();
        }
    }

    // Animation Event: call at the last frame of Opening
    public void SFX_OpenEnd()
    {
        if (sfxA != null) sfxA.Stop();
        if (sfxB != null) sfxB.Stop();
    }
}
