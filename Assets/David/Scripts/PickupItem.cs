using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] public int itemValue = 10;
    [SerializeField] private float throwForce = 10f;

    private Rigidbody rb;
    private Collider col;
    private PlayerController thrownBy;

    public bool IsThrown { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // P¯edmÏt se neh˝be dokud leûÌ na zemi
        Freeze();
    }

    public void PickUp(Transform holdPoint)
    {
        IsThrown = false;
        thrownBy = null;
        Freeze();
        col.enabled = false;
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
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

        // Ignoruj hr·Ëe kter˝ hodil
        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player != null && player == thrownBy) return;

        // Trefil jinÈho hr·Ëe ñ stun
        if (player != null)
            player.Stun();

        // Vûdy zniËit po dopadu (aù trefil kohokoliv nebo cokoliv)
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