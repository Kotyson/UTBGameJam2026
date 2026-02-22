using TMPro;
using UnityEngine;

public class gameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI held1, held2;
    [SerializeField] TextMeshProUGUI total1, total2;

    public void UpdateUI(int p1Held, int p2Held, int p1Total, int p2Total)
    {
        held1.text = p1Held.ToString();
        held2.text = p2Held.ToString();
        total1.text = p1Total.ToString();
        total2.text = p2Total.ToString();
    }
}
