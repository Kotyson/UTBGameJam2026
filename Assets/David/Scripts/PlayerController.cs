using UnityEngine;

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
    [SerializeField] private KeyCode buildKey = KeyCode.E;
    // [SerializeField] private GridManager gridManager;

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

    [SerializeField] public Animator animator;
    
    private float nextAttackTime;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null)
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        currentInputDirection = GetInput();

        if (Input.GetKey(attackKey))
        {
            animator.SetBool("Mining", true);
            TryAttack();
        }
        else
        {
            animator.SetBool("Mining", false);
        }

        if (Input.GetKeyDown(buildKey))
        {
            TryBuild();
        }
    }
    private void FixedUpdate()
    {
        Move(currentInputDirection);
        ClampVelocity();
        RotateTowardsInput();
        UpdateAnimator();
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

        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        return direction.normalized;
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

        if (Physics.SphereCast(origin,
                            sphereRadius,
                            direction,
                            out RaycastHit hit,
                            attackDistance,
                            destructibleLayer,
                            QueryTriggerInteraction.Ignore))
        {
            DestructibleBlock block = hit.collider.GetComponent<DestructibleBlock>();

            if (block != null)
            {
                block.TakeDamage(attackDamage);
            }
        }
    }
    // private void TryBuild()
    // {
    //     if (gridManager == null)
    //         return;

    //     // Always build exactly one grid cell in front
    //     float buildDistance = gridManager.gridSize;

    //     Vector3 buildWorldPos = transform.position + transform.forward * buildDistance;

    //     // Convert world position to grid position using GridManager
    //     Vector3Int gridPos = gridManager.WorldToGrid(buildWorldPos);

    //     // Prevent building inside player's current grid cell
    //     Vector3Int playerGridPos = gridManager.WorldToGrid(transform.position);
    //     if (gridPos == playerGridPos)
    //         return;

    //     gridManager.TryAddBlock(gridPos);
    // }
    private void UpdateAnimator()
    {
        if (animator == null)
            return; 

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        animator.SetFloat("Speed", speed, 0.1f, Time.fixedDeltaTime);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 origin = transform.position + transform.forward * -0.25f;
        Vector3 end = origin + transform.forward * attackDistance;

        Gizmos.DrawWireSphere(origin, sphereRadius);
        Gizmos.DrawWireSphere(end, sphereRadius);
        Gizmos.DrawLine(origin + Vector3.right * sphereRadius, end + Vector3.right * sphereRadius);
        Gizmos.DrawLine(origin - Vector3.right * sphereRadius, end - Vector3.right * sphereRadius);
    }
}