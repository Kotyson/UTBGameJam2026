using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public GemData gem;
    private Rigidbody rb;
    private Collider col;
    
    [Header("Projectile Settings")]
    private bool isProjectile = false;
    private GameObject thrower;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Interact(PlayerController interactor)
    {
        if (interactor != null && interactor.heldItem == null && !isProjectile) // Can't pick up mid-air
        {
            interactor.PickUpItem(this);
        }
    }

    public void OnPickUp(Transform holdPoint)
    {
        isProjectile = false;
        thrower = null;
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnThrow(Vector3 direction, float force, GameObject owner)
    {
        thrower = owner;
        isProjectile = true;

        transform.SetParent(null);
        rb.isKinematic = false;
        col.enabled = true;

        rb.AddForce(direction * force, ForceMode.Impulse);
        rb.AddForce(Vector3.up * (force * 0.5f), ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isProjectile) return;

        // 1. Check for Player
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPlayer))
        {
            if (collision.gameObject != thrower)
            {
                hitPlayer.Stun();
                isProjectile = false; 
            }
        }
        // 2. Check for Chest
        else if (collision.gameObject.TryGetComponent<Chest>(out Chest hitChest))
        {
            Debug.Log("Hit a chest! Maybe deposit item or play a 'clink' sound.");
        
            // Example: If you want the item to automatically enter the chest on hit
            hitChest.DepositItem(this); 
        
            isProjectile = false;
        }
        // 3. Hit anything else (Walls, Floor)
        else
        {
            // Use rb.linearVelocity for Unity 2023+ or rb.velocity for older versions
            if (rb.linearVelocity.magnitude < 2f) 
            {
                isProjectile = false;
            }
        }
    }
}
