using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public PlayerController player1; // Changed to script reference for easier access
    public PlayerController player2; // Changed to script reference for easier access
    
    [Header("Timings")]
    public float deathDelay = 2f;    // Time spent "dead" before appearing at spawn
    public float spawnDuration = 2f; // Time spent "pulsing" before player can move

    // Called via UnityEvent or directly from PlayerController
    public void OnPlayerDeath(PlayerController player)
    {
        StartCoroutine(RespawnSequence(player));
    }

    private IEnumerator RespawnSequence(PlayerController player)
    {
        // 1. Wait while player is "dead"
        yield return new WaitForSeconds(deathDelay);

        // 2. Teleport to spawn
        player.transform.position = player.spawnPoint.position;
        
        // 3. Start the "Spawning" phase (Pulse + No Move)
        yield return StartCoroutine(player.RespawnEffect(spawnDuration));

        // 4. Sequence complete! Player can move again (handled inside player.RespawnEffect)
        Debug.Log("Player Respawned and Ready!");
    }
}