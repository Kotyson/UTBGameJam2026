using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;

public class Live2DMouseFollow : MonoBehaviour
{
    private CubismModel model;
    private CubismParameter movX;
    private CubismParameter movY;

    void Start()
    {
        model = GetComponent<CubismModel>();

        foreach (var p in model.Parameters)
        {
            if (p.Id == "movX")
                movX = p;

            if (p.Id == "movY")
                movY = p;
        }
    }

    void LateUpdate()
    {
        if (movX == null || movY == null)
            return;

        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        float normalizedX = (mouseX - 0.5f) * 2f;
        float normalizedY = (mouseY - 0.5f) * 2f;

        // Use parameter's actual range
        float rangeX = movX.MaximumValue - movX.MinimumValue;
        float rangeY = movY.MaximumValue - movY.MinimumValue;

        movX.Value = normalizedX * (rangeX / 2f);
        movY.Value = normalizedY * (rangeY / 2f);
    }
}