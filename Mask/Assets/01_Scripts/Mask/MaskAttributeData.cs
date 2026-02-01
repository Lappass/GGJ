using UnityEngine;

// Define the attribute types
public enum AttributeType
{
    Emotion,
    Identity
}

// Define specific values for Emotions and Identities
public enum EmotionType
{
    None,
    Happy,
    Sad,
    Angry,
    Fearful,
    Understanding,
}

public enum IdentityType
{
    Journalist,
    Detective,
    Therapist,
    DirtyCop,
    None
}

[CreateAssetMenu(fileName = "NewMaskAttribute", menuName = "Mask/Attribute Data")]
public class MaskAttributeData : ScriptableObject
{
    [Header("Attribute Configuration")]
    public AttributeType type;
    
    [Header("Values")]
    public EmotionType emotionValue;
    public IdentityType identityValue;

    [Header("Visuals")]
    public Sprite icon;
    public MaskPosition defaultPosition; // Where does this fragment usually go?

    [TextArea]
    public string description;

    public string GetDisplayText()
    {
        if (type == AttributeType.Emotion)
            return $"Emotion: {emotionValue}\n{description}";
        else
            return $"Identity: {identityValue}\n{description}";
    }
}

