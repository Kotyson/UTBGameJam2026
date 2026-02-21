using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleTopDownController : MonoBehaviour
{
    public enum ControlType
    {
        WASD,
        ArrowKeys
    }

    [Header("Control Settings")]
    public ControlType controlType;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Vector3 inputDirection = GetInput();
        Move(inputDirection);
        ClampVelocity();
        RotateTowardsMovement();
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
    private void RotateTowardsMovement()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Don't rotate if barely moving
        if (horizontalVelocity.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity.normalized, Vector3.up);

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }
}