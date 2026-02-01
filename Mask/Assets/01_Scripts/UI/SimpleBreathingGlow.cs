using UnityEngine;
using UnityEngine.UI;

public class SimpleBreathingGlow : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float speed = 2.0f;
    
    private SpriteRenderer sr;
    private Image img;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        img = GetComponent<Image>();
    }

    private void Update()
    {
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f);
        SetAlpha(alpha);
    }

    private void SetAlpha(float a)
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
        else if (img != null)
        {
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
}

