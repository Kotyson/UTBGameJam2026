using UnityEngine;

public class SharedCamera : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    [Header("Zoom")]
    public float minZoom = 8f;          // Výchozí blízká kamera
    public float maxZoom = 30f;         // Maximum oddálení
    public float zoomStartDistance = 5f; // Od jaké vzdálenosti zaène zoom
    public float zoomSpeed = 2f;        // Jak rychle reaguje

    [Header("Smooth")]
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;
    private float currentZoom;
    private Quaternion fixedRotation;

    void Start()
    {
        fixedRotation = Quaternion.Euler(60f, 0f, 0f);
        transform.rotation = fixedRotation;
        currentZoom = minZoom;
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        //  Støed mezi hráèi
        Vector3 centerPoint = (player1.position + player2.position) / 2f;

        //  Vzdálenost mezi hráèi
        float distance = Vector3.Distance(player1.position, player2.position);

        //  Zoom zaène až po urèité vzdálenosti
        float targetZoom = minZoom;

        if (distance > zoomStartDistance)
        {
            float extraDistance = distance - zoomStartDistance;
            targetZoom = Mathf.Clamp(minZoom + extraDistance, minZoom, maxZoom);
        }

        //  Plynulé pøibližování/oddalování
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSpeed);

        //  Offset kamery
        Vector3 offset = new Vector3(0, 1f, -0.6f).normalized * currentZoom;
        Vector3 targetPosition = centerPoint + offset;

        //  Smooth pohyb
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        transform.rotation = fixedRotation;
    }
}