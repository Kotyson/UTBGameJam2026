using UnityEngine;

public interface IInteractable
{
    // We pass the interactor so the chest knows which player to give the loot to!
    void Interact(PlayerController interactor); 
}