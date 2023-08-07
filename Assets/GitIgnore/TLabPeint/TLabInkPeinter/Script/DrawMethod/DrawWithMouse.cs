using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWithMouse : MonoBehaviour
{
    // -----------------------------
    // �}�E�X����ł̃y�C���g����

    public CanvasServer canvas;

    public void DrawWithMouseAction()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray;
            RaycastHit hit;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Collider�Ƃ̓����蔻��ňʒu����肷��
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                canvas.DrawFromMouseAction(hit.textureCoord);
            }
        }
    }
}
