using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public enum ControlType
    {
        WASD,
        ArrowKeys
    }

    [Header("Control Settings")]
    public ControlType controlType;
    private Vector3 currentInputDirection;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Attack Settings")]
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRate = 4f;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    [SerializeField] private LayerMask destructibleLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float knockbackForce = 12f;

    [Header("Item Settings")]
    [SerializeField] private KeyCode pickupThrowKey = KeyCode.E;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private GameObject pickaxeVisual;

    [SerializeField] public Animator animator;

    private float nextAttackTime;
    private Rigidbody rb;
    private PickupItem heldItem;
    private Chest nearbyChest;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        currentInputDirection = GetInput();

        // Attack � pouze pokud hr�� nem� p�edm�t v ruce
        if (heldItem == null)
        {
            if (Input.GetKey(attackKey))
            {
                animator.SetBool("Mining", true);
                TryAttack();
            }
            else
            {
                animator.SetBool("Mining", false);
            }
        }
        else
        {
            animator.SetBool("Mining", false);
        }

        HandleItemInput();
    }

    private void FixedUpdate()
    {
        Move(currentInputDirection);
        ClampVelocity();
        RotateTowardsInput();
        UpdateAnimator();
    }

    

    private void HandleItemInput()
    {
        if (!Input.GetKeyDown(pickupThrowKey)) return;

        if (heldItem != null)
        {
            if (nearbyChest != null)
            {
                // Dej p�edm�t do chestky
                nearbyChest.DepositItem(heldItem);
                heldItem = null;
            }
            else
            {
                // Ho� p�edm�t p�ed sebe
                Vector3 throwDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
                heldItem.Drop(throwDir, this);
                heldItem = null;
            }

            // Vra� krump��
            if (pickaxeVisual != null)
                pickaxeVisual.SetActive(true);
        }
        else
        {
            // Pokus o sebr�n� nejbli���ho p�edm�tu
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, itemLayer);
            if (hits.Length == 0) return;

            Collider nearest = hits[0];
            float minDist = float.MaxValue;
            foreach (var h in hits)
            {
                float d = Vector3.Distance(transform.position, h.transform.position);
                if (d < minDist) { minDist = d; nearest = h; }
            }

            PickupItem item = nearest.GetComponent<PickupItem>();
            if (item == null) return;

            heldItem = item;
            heldItem.PickUp(holdPoint);

            // Skryj krump��
            if (pickaxeVisual != null)
                pickaxeVisual.SetActive(false);
        }
    }

    public void SetNearChest(Chest chest)
    {
        nearbyChest = chest;
    }

    public void Stun()
    {
        Debug.Log($"[Player] Hr�� {controlType} stunnut�!");
        // TODO: implementace stunu (zablokovat input, p�ehr�t animaci, atd.)
        animator.SetBool("Stun", true);
        // rotate the player against the direction of the movement of the rigidbody
            Vector3 velocity = rb.linearVelocity; 
            if (velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-velocity.normalized, Vector3.up);
                rb.rotation = targetRotation;
            }
        

        
        // here there should be some corrutine to set stun false
        StartCoroutine(ResetStun());
    }
    public IEnumerator ResetStun()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetBool("Stun", false);
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }


    

    private Vector3 GetInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (controlType == ControlType.WASD)
        {
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
        }
        else if (controlType == ControlType.ArrowKeys)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
        }

        return new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void Move(Vector3 direction)
    {
        rb.AddForce(direction * moveForce, ForceMode.Force);
    }

    private void ClampVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void RotateTowardsInput()
    {
        if (currentInputDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(currentInputDirection, Vector3.up);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + 1f / attackRate;

        Vector3 origin = transform.position + transform.forward * -0.25f;
        Vector3 direction = transform.forward;

        // Tref bloky i hr��e jedn�m castem
        if (Physics.SphereCast(origin,
                                sphereRadius,
                                direction,
                                out RaycastHit hit,
                                attackDistance,
                                destructibleLayer | playerLayer,
                                QueryTriggerInteraction.Ignore))
        {
            DestructibleBlock block = hit.collider.GetComponent<DestructibleBlock>();
            if (block != null)
                block.TakeDamage(attackDamage);

            PlayerController otherPlayer = hit.collider.GetComponent<PlayerController>();
            if (otherPlayer != null && otherPlayer != this)
            {
                Vector3 knockbackDir = new Vector3(direction.x, 0f, direction.z).normalized;
                otherPlayer.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
    }

    

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        animator.SetFloat("Speed", speed, 0.1f, Time.fixedDeltaTime);
    }

    

    private void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Vector3 origin = transform.position + transform.forward * -0.25f;
        Vector3 end = origin + transform.forward * attackDistance;
        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawWireSphere(end, sphereRadius);
        Gizmos.DrawLine(origin + Vector3.right * sphereRadius, end + Vector3.right * sphereRadius);
        Gizmos.DrawLine(origin - Vector3.right * sphereRadius, end - Vector3.right * sphereRadius);

        // Pickup radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}