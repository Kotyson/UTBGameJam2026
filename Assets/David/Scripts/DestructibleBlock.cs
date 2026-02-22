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
    private bool isDestroying = false;
    public bool isFallingHazard = false;

    [Header("VFX Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.1f;

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
        transform.localScale = originalScale;
        isDestroying = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDestroying) return;

        AudioManager.Instance.Play("Mining");
        currentHealth -= amount;

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());

        if (currentHealth <= 0)
        {
            isDestroying = true;
            gridManager.InitiateDestruction(gridPosition);
            StartCoroutine(ShrinkAndDestroy());
        }
    }

    private IEnumerator ShrinkAndDestroy()
    {
        AudioManager.Instance.Play("Rock_Destroy_1");

        Collider col = GetComponentInChildren<Collider>();
        if (col != null) col.enabled = false;

        Vector3 targetScale = Vector3.zero;
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10f);
            yield return null;
        }

        HandleLootDrop();

        yield return null;

        gridManager.FinishDestruction(gridPosition);
    }

    private void HandleLootDrop()
    {
        if (blockData == null || blockData.dropTable == null)
        {
            Debug.LogWarning($"Blok '{gameObject.name}' nem� p�i�azen� DropTable!");
            return;
        }

        DropTable table = blockData.dropTable;

        // --- PEN�ZE: p�i�teme hr��i do kapsy ---
        // Najdeme nejbli���ho hr��e v okol� (ten kdo t�il)
        PlayerController nearestPlayer = FindNearestPlayer();
        if (nearestPlayer != null)
        {
            int money = table.RollMoney();
            nearestPlayer.AddMoney(money);
        }

        // --- GEM ---
        GameObject gem = table.RollGem();
        if (gem == null)
        {
            Debug.Log("��dn� gem nevypadl.");
            return;
        }

        // if (gem.prefab == null)
        // {
        //     Debug.LogError($"Gem '{gem.gemName}' nem� p�i�azen� prefab!");
        //     return;
        // }

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject spawned = Instantiate(gem, spawnPos, Quaternion.identity);
        spawned.transform.SetParent(null);
        Debug.Log($"Gem spawnut: '{gem.name}' na {spawned.transform.position}");
    }

    private PlayerController FindNearestPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController nearest = null;
        float minDist = float.MaxValue;

        foreach (PlayerController p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p;
            }
        }

        return nearest;
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