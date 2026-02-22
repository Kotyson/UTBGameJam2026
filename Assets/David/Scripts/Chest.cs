using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private static int totalPoints = 0;
    [SerializeField] private PlayerController owner;

    public void DepositItem(PickupItem item)
    {
        int value = 10; // TODO - add real value
        totalPoints += value;
        Debug.Log($"[Chest] Predmet vlozen! +{value} bode | Celkem: {totalPoints}");
        Destroy(item.gameObject);
    }

    public void DepositMoney(int amount)
    {
        totalPoints += amount;
        Debug.Log($"[Chest] Pen�ze vlo�eny! +{amount} | Celkem: {totalPoints}");
    }
    
    public void Interact(PlayerController interactor)
    {
        if (interactor == null || interactor != owner)
        {
            Debug.Log("Not owner");
            return;
        }
        Debug.Log("Owner is interacting");
        if (owner.heldItem != null)
        {
            DepositItem(owner.heldItem);
        }
    }
}
