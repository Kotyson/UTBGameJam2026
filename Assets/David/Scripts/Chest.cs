using UnityEngine;

public class Chest : MonoBehaviour
{
    private static int totalPoints = 0;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.SetNearChest(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.SetNearChest(null);
    }

    public void DepositItem(PickupItem item)
    {
        int value = item.itemValue;
        totalPoints += value;
        Debug.Log($"[Chest] Pøedmìt vložen! +{value} bodù | Celkem: {totalPoints}");
        Destroy(item.gameObject);
    }
}