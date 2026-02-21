using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockData blockData;

    [Header("Health Height Mapping")]
    [SerializeField] private float fullHealthHeight = 0f;      // offset at 100%
    [SerializeField] private float zeroHealthHeight = -1f;     // offset at 0%

    [Header("Effects")]
    [SerializeField] private float shakeAmount = 0.05f;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float sinkSmoothSpeed = 8f;

    private float currentHealth;
    private Vector3 basePosition;
    private float velocity;
    private Vector3Int gridPosition;
    private GridManager gridManager;

    private float targetYOffset;
    private bool isDestroying = false;

    private void Awake()
    {
        currentHealth = blockData.maxHealth;
        basePosition = transform.position;
        targetYOffset = fullHealthHeight;
    }

    public void Initialize(GridManager manager, Vector3Int pos)
    {
        gridManager = manager;
        gridPosition = pos;
    }

    private void Update()
    {
        // Smooth sinking movement
        Vector3 targetPos = basePosition + Vector3.up * targetYOffset;
        //transform.position = Vector3.Lerp(transform.position, targetPos, sinkSmoothSpeed * Time.deltaTime);

        // transform.position = Vector3.SmoothDamp(
        //     transform.position,
        //     targetPos,
        //     ref velocity,
        //     0.1f
        // );
    }

    public void TakeDamage(float amount)
    {
        if (isDestroying) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, blockData.maxHealth);

        float healthPercent = currentHealth / blockData.maxHealth;

        // Map health % to height
        targetYOffset = Mathf.Lerp(zeroHealthHeight, fullHealthHeight, healthPercent);

        StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            isDestroying = true;
            StartCoroutine(RemoveAfterDelay());
        }
    }

    private IEnumerator Shake()
    {
        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            Vector3 randomOffset = Random.insideUnitSphere * shakeAmount;
            randomOffset.y = 0f;

            transform.position += randomOffset;

            yield return null;
        }
    }

    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);

        gridManager.RemoveBlock(gridPosition);
    }
}