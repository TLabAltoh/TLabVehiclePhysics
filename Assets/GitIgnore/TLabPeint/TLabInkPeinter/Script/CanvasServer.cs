using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasServer : MonoBehaviour
{
    // ------------------------------------------------------------------------------------
    // �L�����o�X�ɂ����� GameObject�ɂ��̃R���|�[�l���g���A�^�b�`����
    // �e�v���C���[����̕`�施�߂����̃N���X����������̂ŃT�[�o�[�̂悤�Ȗ������ʂ���

    public Material inputBuffer_material;

    private Texture input;
    private Texture inputNow;
    public Texture2D StampTexture;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public Drawing drawing;
    private CalculateUVOnCollision calcUV;

    private int waitCount = 0;
    private int wait = 3;

    // �����S��Mode(�܂��������ĂȂ�)
    private bool kesigomu;


    public void DrawFromMouseAction(Vector2 hitUv)
    {
        drawing.DrawPixelStamp(hitUv);
    }


    public void DrawFromGunAction(Vector3 laserGunHitPoint)
    {        
        Vector2 uv = calcUV.CalculateUV(laserGunHitPoint, meshFilter, transform);
        drawing.DrawPixelStamp(uv);
    }


    void OnCollisionStay(Collision collision)
    {
        // OnCollisionStay����Ȃ���uv�𐳂����Z�o�o���Ȃ�����
        // ���������u�Ԃ̍��W�𐳂����擾�ł��Ă��Ȃ��̂��C�ɂȂ�
        // OnCollisionEnter�ł́A�ŏ��̏Փ˂̓t���[���̌덷�̊֌W�Ő������擾�ł��Ȃ�
        // OnCollisionStay�ł�����

        if (waitCount > wait)
        {
            waitCount = 0;

            foreach (var p in collision.contacts)
            {
                Vector2 uv = calcUV.CalculateUV(p.point, meshFilter, transform);
                if (uv != new Vector2(2, 2))
                {
                    drawing.DrawPixel(uv);
                }
            }
        }

        waitCount++;
    }


    public void GetBuffer()
    {
        input = inputBuffer_material.GetTexture("input");
        inputNow = inputBuffer_material.GetTexture("input_now");
    }


    void Start()
    {
        // �ŏ��A�X�^���v�ɂ������e�N�X�`����Grahpics�֐��Ń��T�C�Y���Ă���g�����Ƃ������A
        // ���T�C�Y�����f�[�^��GPU�������ɂ����ێ�����Ă��Ȃ��Ƃ������Ƃ����o�����̂ł�߂�
        // (�Q�l����_�uGetPixels()��GPU�������v���Q��)

        // �d�v��Script���擾����
        drawing = new Drawing();
        calcUV = new CalculateUVOnCollision();

        // �X�^���v�Ɏg���e�N�X�`����Drawing�Ɉ����n��
        Debug.Log("stampTexture_befor.isReadable = " + StampTexture.isReadable);
        drawing.StampTexture = StampTexture;
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        drawing.MeshRenderer = meshRenderer;
        drawing.CreateReFleshTexture(inputBuffer_material);
    }


    void Update()
    {
        // GUI��Debug���邽�߂�Buffer���擾
        GetBuffer();
    }


#if false

    void OnGUI()
    {
        // simpleInkPainter��Debug����
        int h = Screen.height / 3;
        int w = Screen.Width;

        GUI.DrawTexture(new Rect(0, 0 * h, h, h), input);
        GUI.DrawTexture(new Rect(0, 1 * h, h, h), inputNow);
        GUI.Box(new Rect(0, 1 * h - h / 10, h, h / 10), "input");
        GUI.Box(new Rect(0, 3 * h - h / 10, h, h / 10), "inputNow");
        kesigomu = GUI.Toggle(new Rect(w - h, 3 * h - h / 6, h, h / 6), kesigomu, "�����S�����[�h");
    }

#endif
}
