using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockData blockData;

    private Vector3Int gridPosition;
    private GridManager gridManager;

    private float currentHealth;

    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private Coroutine shakeRoutine;
    private Vector3 originalLocalPos;

    private void Awake()
    {
        currentHealth = blockData.maxHealth;
        originalLocalPos = transform.localPosition;
    }

    public void Initialize(GridManager manager, Vector3Int pos)
    {
        gridManager = manager;
        gridPosition = pos;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // Trigger shake
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            gridManager.RemoveBlock(gridPosition);
        }
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetZ = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition = originalLocalPos + new Vector3(offsetX, 0f, offsetZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
    }
}