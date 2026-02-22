using UnityEngine;

public class gem : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController interactor)
    {
        Debug.Log("Gem");
    }
}
