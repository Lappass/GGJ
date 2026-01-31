using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract int Priority { get; }

    public virtual bool CanInteract => true;

    public abstract void Interact(PlayerInteractor interactor);
}
