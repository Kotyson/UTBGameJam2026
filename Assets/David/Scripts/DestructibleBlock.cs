using UnityEngine;
using System.Collections;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Block Settings")]
    public BlockData blockData;

    private Vector3Int gridPosition;
    private GridManager gridManager;
    private float currentHealth;
    [Header("Status")]
    private bool isDestroying = false; // Prevents double-triggering
    public bool isFallingHazard = false;

    [Header("VFX Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private float shrinkSpeed = 5f; // How fast it "pufs" away

    private Coroutine shakeRoutine;
    private Vector3 originalLocalPos;
    private Vector3 originalScale;

    private void Awake()
    {
        currentHealth = blockData.maxHealth;
        originalLocalPos = transform.localPosition;
        originalScale = transform.localScale;
    }

    public void Initialize(GridManager manager, Vector3Int pos)
    {
        gridManager = manager;
        gridPosition = pos;
        
        // Reset scale in case it was pooled or reused
        transform.localScale = originalScale;
        isDestroying = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDestroying) return;

        currentHealth -= amount;
    
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            isDestroying = true;
        
            // 1. Tell GridManager to remove it from LOGIC immediately
            // This makes neighbors update their meshes NOW.
            gridManager.InitiateDestruction(gridPosition);

            // 2. Start the visual puff/shrink
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    private IEnumerator ShrinkAndDestroy()
    {
        // Disable collider so player/NPCs don't bump into a "ghost" block
        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        // Visual shrink animation
        Vector3 targetScale = Vector3.zero;
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
            yield return null;
        }

        // 3. Finally, tell GridManager to destroy the object and start the respawn timer
        gridManager.FinishDestruction(gridPosition);
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