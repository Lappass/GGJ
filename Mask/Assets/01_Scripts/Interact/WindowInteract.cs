using UnityEngine;

public class WindowInteract : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    private bool isOpen = false;

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (anim == null) return;

        if (!isOpen)
        {
            anim.ResetTrigger(closeTrigger);
            anim.SetTrigger(openTrigger);
            isOpen = true;
        }
        else
        {
            anim.ResetTrigger(openTrigger);
            anim.SetTrigger(closeTrigger);
            isOpen = false;
        }
    }
}
