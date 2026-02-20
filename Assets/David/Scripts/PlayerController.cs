using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 8f;
    public float rotationSpeed = 720f;

    public Transform otherPlayer;
    public SharedCamera cameraScript;

    public string horizontalAxis = "Horizontal1";
    public string verticalAxis = "Vertical1";

    private Camera mainCamera;
    private float margin = 0.08f; // Rezerva od okraje (0.0 az 1.0)

    void Start()
    {
        mainCamera = Camera.main;
        if (cameraScript == null) cameraScript = mainCamera.GetComponent<SharedCamera>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw(horizontalAxis);
        float moveZ = Input.GetAxisRaw(verticalAxis);

        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            // 1. Plynul· rotace (nez·visl· na tom, jestli se h˝beme)
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 2. V˝poËet pohybu po os·ch
            // ZkouöÌme X a Z zvl·öù, aby n·s blok na jednÈ ose nezastavil ˙plnÏ
            Vector3 movementX = new Vector3(inputDirection.x, 0, 0) * speed * Time.deltaTime;
            Vector3 movementZ = new Vector3(0, 0, inputDirection.z) * speed * Time.deltaTime;

            Vector3 finalMovement = Vector3.zero;

            // Kontrola pro X osu
            if (CanMove(transform.position + movementX))
                finalMovement += movementX;

            // Kontrola pro Z osu
            if (CanMove(transform.position + finalMovement + movementZ))
                finalMovement += movementZ;

            transform.position += finalMovement;
        }
    }

    bool CanMove(Vector3 potentialPos)
    {
        // Kde by byl st¯ed a kamera po mÈm pohybu
        Vector3 potentialCenter = (potentialPos + otherPlayer.position) / 2f;
        Vector3 potentialCamPos = potentialCenter + cameraScript.offset;

        // ProvÏ¯Ìme viditelnost obou hr·Ë˘ v novÈ konfiguraci
        bool meOk = IsInViewport(potentialPos, potentialCamPos);
        bool partnerOk = IsInViewport(otherPlayer.position, potentialCamPos);

        // Z¡CHRANN¡ LOGIKA: 
        // Pokud mÏ pohyb vracÌ blÌû ke st¯edu (k druhÈmu hr·Ëi), dovolÌme ho vûdy.
        // TÌm se p¯edejde zaseknutÌ "mimo" kameru.
        float currentDist = Vector3.Distance(transform.position, otherPlayer.position);
        float potentialDist = Vector3.Distance(potentialPos, otherPlayer.position);

        if (potentialDist < currentDist) return true;

        return meOk && partnerOk;
    }

    bool IsInViewport(Vector3 worldPos, Vector3 camPos)
    {
        Vector3 oldPos = mainCamera.transform.position;
        mainCamera.transform.position = camPos;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(worldPos);

        mainCamera.transform.position = oldPos;

        // Kontrola, zda je bod v "bezpeËnÈm obdÈlnÌku" kamery
        return (viewPos.x > margin && viewPos.x < (1 - margin) &&
                viewPos.y > margin && viewPos.y < (1 - margin) &&
                viewPos.z > 0);
    }
}