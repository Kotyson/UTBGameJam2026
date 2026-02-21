using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

public class PickupItem : MonoBehaviour
{
    [SerializeField] public int itemValue = 10;
    [SerializeField] private float throwForce = 10f;

    private Rigidbody rb;
    private Collider col;
    private float DestroyAfterCollisionCooldown = 3f;
    private PlayerController thrownBy;
    private bool canBePickedUp = true;

    public bool IsThrown { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // P�edm�t se neh�be dokud le�� na zemi
        //Freeze();
    }

    public void PickUp(Transform holdPoint)
    {
        if (!canBePickedUp) return;
        IsThrown = false;
        thrownBy = null;
        Freeze();
        col.enabled = false;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        //Dont allow picking up if the item is in cooldown of destroying after collision

        
    }

    public void Drop(Vector3 throwDirection, PlayerController thrower)
    {
        transform.SetParent(null);
        IsThrown = true;
        thrownBy = thrower;
        Unfreeze();
        col.enabled = true;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsThrown) return;

        // Ignoruj hr��e kter� hodil
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player != null && player == thrownBy) return;

        // Trefil jin�ho hr��e � stun
        if (player != null)
            player.Stun();
        
        // V�dy zni�it po dopadu (a� trefil kohokoliv nebo cokoliv)
       // destroy after cooldown
       StartCoroutine(DestroyAfterCooldown());

    }
    public IEnumerator DestroyAfterCooldown()
    {
        yield return new WaitForSeconds(DestroyAfterCollisionCooldown);
        canBePickedUp = false;
        Destroy(gameObject);
    }

    private void Freeze()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void Unfreeze()
    {
        rb.isKinematic = false;
    }
}