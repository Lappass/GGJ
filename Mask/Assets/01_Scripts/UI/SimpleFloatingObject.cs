using UnityEngine;

public class SimpleFloatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatAmplitude = 0.1f; // Height of the float
    [SerializeField] private float floatFrequency = 1f;   // Speed of the float
    
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    private void Update()
    {
        // Calculate new Y position using Sin wave
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}

