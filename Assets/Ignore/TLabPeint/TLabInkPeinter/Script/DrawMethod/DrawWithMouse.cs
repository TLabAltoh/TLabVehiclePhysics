using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWithMouse : MonoBehaviour
{
    // -----------------------------
    // マウス操作でのペイント処理

    public CanvasServer canvas;

    public void DrawWithMouseAction()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Colliderとの当たり判定で位置を特定する
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                canvas.DrawFromMouseAction(hit.textureCoord);
            }
        }
    }
}
