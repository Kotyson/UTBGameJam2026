using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float rotationSpeed = 720f;
    public string horizontalAxis = "Horizontal1";
    public string verticalAxis = "Vertical1";

    private Camera mainCamera;
    private float margin = 0.03f; // 3% rezerva od okraje obrazovky

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveZ = Input.GetAxisRaw(verticalAxis);
        Vector3 moveInput = new Vector3(moveX, 0, moveZ).normalized;

        // 1. Pohyb
        if (moveInput.magnitude >= 0.1f)
        {
            // Otoèení
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Samotný pohyb
            transform.position += moveInput * speed * Time.deltaTime;
        }

        // 2. TVRDÝ STOP (Hranice kamery)
        // Pøevedeme pozici hráèe do "Viewportu" (0 = vlevo/dole, 1 = vpravo/nahoøe)
        Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);

        // Pokud vyboèíme, vrátíme se na hranici
        bool clamped = false;
        if (viewPos.x < margin) { viewPos.x = margin; clamped = true; }
        if (viewPos.x > 1 - margin) { viewPos.x = 1 - margin; clamped = true; }
        if (viewPos.y < margin) { viewPos.y = margin; clamped = true; }
        if (viewPos.y > 1 - margin) { viewPos.y = 1 - margin; clamped = true; }

        if (clamped)
        {
            // Pokud jsme byli mimo, vypoèítáme novou pozici ve svìtì
            // Výška (Z ve Viewportu) musí zùstat stejná, aby postava "neuskakovala" nahoru
            float distanceToCam = viewPos.z;
            Vector3 worldPos = mainCamera.ViewportToWorldPoint(viewPos);

            // Zachováme Y pozici hráèe (aby nezaèal levitovat)
            transform.position = new Vector3(worldPos.x, transform.position.y, worldPos.z);
        }
    }
}