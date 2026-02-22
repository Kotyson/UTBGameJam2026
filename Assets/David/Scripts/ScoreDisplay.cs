using UnityEngine;
using TMPro; // Nezapomeò na tento namespace pro TextMeshPro

public class ScoreDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    void Start()
    {
        // Jakmile se scéna naète, vytáhneme data ze statické tøídy
        DisplayScores();
    }

    void DisplayScores()
    {
        // Formátování textu pøesnì podle tvého zadání
        player1Text.text = "Hráè jedna: " + LevelData.Player1Score;
        player2Text.text = "Hráè dva: " + LevelData.Player2Score;

        Debug.Log($"Zobrazeno skóre - P1: {LevelData.Player1Score}, P2: {LevelData.Player2Score}");
    }
}