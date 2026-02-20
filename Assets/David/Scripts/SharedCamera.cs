using UnityEngine;

public class SharedCamera : MonoBehaviour
{
    public Transform player1;
    public Transform player2;

    public Vector3 offset = new Vector3(0f, 15f, -15f);
    public float smoothTime = 0.15f; // Trochu svižnìjší sledování

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        Vector3 centerPoint = GetCenterPoint();
        Vector3 targetPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        transform.LookAt(centerPoint);
    }

    public Vector3 GetCenterPoint()
    {
        Bounds bounds = new Bounds(player1.position, Vector3.zero);
        bounds.Encapsulate(player2.position);
        return bounds.center;
    }
}