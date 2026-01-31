using UnityEngine;
using TMPro;

public class MaskAttributeDisplay : MonoBehaviour
{
    public static MaskAttributeDisplay Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI attributeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Hide on start
        ClearAttributes();
    }

    public void ShowAttributes(string text)
    {
        if (attributeText != null)
        {
            attributeText.gameObject.SetActive(true);
            attributeText.text = text;
        }
    }

    public void ClearAttributes()
    {
        if (attributeText != null)
        {
            attributeText.text = "";
            attributeText.gameObject.SetActive(false);
        }
    }
}

