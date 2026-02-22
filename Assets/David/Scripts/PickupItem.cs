using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
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
        // Only trigger if we are currently a flying projectile
        if (!isProjectile) return;

        // Did we hit a player?
        PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();

        if (hitPlayer != null)
        {
            // Don't stun the person who threw it!
            if (collision.gameObject != thrower)
            {
                hitPlayer.Stun(); // Call your existing stun function
                Debug.Log(hitPlayer.name + " was stunned by " + gameObject.name);
                
                // End projectile state on hit
                isProjectile = false; 
            }
        }
        else
        {
            // If we hit a wall or floor, it's no longer a "dangerous" projectile
            // We use a small delay or check velocity so it doesn't stun after bouncing
            if (rb.velocity.magnitude < 2f) 
            {
                isProjectile = false;
            }
        }
    }
}