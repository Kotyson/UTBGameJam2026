using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public enum ControlType
    {
        WASD,
        ArrowKeys
    }

    [Header("Control Settings")] public ControlType controlType;
    private Vector3 currentInputDirection;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float footstepDelay = 0.5f;
    private float footstepTimer;

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
    [SerializeField] private float pickupRadius = 1.5f;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private GameObject pickaxeVisual;
    
    [Header("Interaction Settings")]
    public float interactDistance = 1.5f;
    public float interactRadius = 0.5f;
    public LayerMask interactLayer;

    [Header("Money")]
    public int carriedMoney = 0;

    [Header("Status")]
    [SerializeField] private bool isDead = false;
    [SerializeField] public Animator animator;
    [SerializeField] private Transform characterVisual;
    private bool canMove = true;
    private Renderer playerRenderer;
    private Color originalColor;
    public Transform spawnPoint;

    private float nextAttackTime;
    private Rigidbody rb;
    
    [Header("Throw Settings")]
    public Transform holdPoint;
    public float throwForce = 10f;
    public PickupItem heldItem;

    [Header("Events")] public UnityEvent onDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        playerRenderer = GetComponentInChildren<Renderer>();
        originalColor = playerRenderer.material.color;
    }

    private void Update()
    {
        if (!canMove) return;
        currentInputDirection = GetInput();

        if (!isStunned && heldItem == null)
        {
            if (Input.GetKey(attackKey))
                TryAttack();
            else
                animator.SetBool("Mining", false);
        }
        else
        {
            animator.SetBool("Mining", false);
        }

        if (!isStunned) HandleItemInput();

        HandleFootsteps();
    }

    public void AddMoney(int value)
    {
        carriedMoney += value;
    }
    
    private void FixedUpdate()
    {
        Move(currentInputDirection);
        ClampVelocity();
        RotateTowardsInput();
        UpdateAnimator();
    }

    private void HandleFootsteps()
    {
        bool isMoving = currentInputDirection.sqrMagnitude > 0.01f && rb.linearVelocity.magnitude > 0.5f;

        if (isMoving && !isDead && !isStunned)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                // AudioManager.Instance.Play("Steps_1");
                footstepTimer = footstepDelay;
            }
        }
        else
        {
            footstepTimer = 0;
        }
    }
    
    private void HandleItemInput()
    {
        if (!Input.GetKeyDown(pickupThrowKey)) return;
        
        // 1. If we are holding something and NOT looking at an interactable, we throw it
        if (heldItem != null)
        {
            // Try to find a chest/container first
            IInteractable target = GetNearestInteractable();
        
            if (target != null && target is Chest) // Special case if you want to prioritize depositing
            {
                target.Interact(this);
            }
            else
            {
                ThrowItem();
            }
        }
        // 2. If we are empty-handed, we look for the nearest interactable (Item, Chest, Lever, etc.)
        else
        {
            IInteractable target = GetNearestInteractable();
            if (target != null)
            {
                Debug.Log("Interagujeme");
                target.Interact(this);
            }
        }
    }

    public void PickUpItem(PickupItem item)
    {
        heldItem = item;
        heldItem.OnPickUp(holdPoint);
    
        // If you have a pickaxe visual, hide it while holding a rock
        if (pickaxeVisual != null) pickaxeVisual.SetActive(false);
    }

    private void ThrowItem()
    {
        AudioManager.Instance.Play("Rock_Throw");
    
        Vector3 throwDir = transform.forward;
    
        // Pass 'gameObject' as the 3rd argument (the thrower)
        heldItem.OnThrow(throwDir, throwForce, gameObject); 
    
        heldItem = null;

        if (pickaxeVisual != null) pickaxeVisual.SetActive(true);
    }
    
    private IInteractable GetNearestInteractable()
    {
        // Use your OverlapSphere approach to find everything nearby
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, interactLayer);
    
        IInteractable nearest = null;
        float minDist = float.MaxValue;

        foreach (var h in hits)
        {
            IInteractable interactable = h.GetComponentInParent<IInteractable>();
            Debug.Log("Interactable" + interactable == null);
            if (interactable == null) continue;

            float d = Vector3.Distance(transform.position, h.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = interactable;
            }
        }
        return nearest;
    }
    
    
    private void OnDrawGizmosSelected()
    {
        // Make sure these match your actual variables!
        float interactRadius = this.interactRadius; 
        float interactDistance = this.interactDistance;
    
        Vector3 origin = transform.position + transform.forward * -0.25f;
        Vector3 direction = transform.forward;
        origin.y -= 0.5f;
        
        Gizmos.color = Color.yellow;
        // Draw the line of the cast
        Gizmos.DrawRay(origin, direction * interactDistance);
        // Draw the sphere at the end so you can see its size
        Gizmos.DrawWireSphere(origin + direction * interactDistance, interactRadius);
    }

    // public void SetNearChest(Chest chest)
    // {
    //     nearbyChest = chest;
    // }

    private bool isStunned = false;

    public void Stun()
    {
        AudioManager.Instance.Play("Grunt_1");
        animator.SetBool("Stun", true);

        Vector3 velocity = rb.linearVelocity;
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-velocity.normalized, Vector3.up);
            rb.rotation = targetRotation;
        }

        if (isStunned)
            StopCoroutine(ResetStun());

        StartCoroutine(ResetStun());
    }

    private IEnumerator ResetStun()
    {
        isStunned = true;
        yield return new WaitForSeconds(1.5f);
        isStunned = false;
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
        Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newRotation);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            animator.SetBool("Mining", false);
            return;
        }

        animator.SetBool("Mining", true);
        nextAttackTime = Time.time + 1f / attackRate;

        Vector3 origin = transform.position + transform.forward * -0.25f;
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, attackDistance, destructibleLayer | playerLayer, QueryTriggerInteraction.Ignore))
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

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.TryGetComponent<DestructibleBlock>(out DestructibleBlock block))
        {
            if (block.isFallingHazard) Die();
        }
    }

    private void Die()
    {
        AudioManager.Instance.Play("Splat");
        canMove = false;
        characterVisual.transform.localScale = new Vector3(1f, 0.1f, 1f);
        isDead = true;
        this.enabled = false;
        onDeath?.Invoke();
    }

    public IEnumerator RespawnEffect(float duration)
    {
        characterVisual.transform.localScale = Vector3.one;
        playerRenderer.enabled = true;
        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        playerRenderer.material.color = originalColor;
        this.enabled = true;
        canMove = true;
        isDead = false;
    }
}
