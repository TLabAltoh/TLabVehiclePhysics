using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasServer : MonoBehaviour
{
    // ------------------------------------------------------------------------------------
    // キャンバスにしたい GameObjectにこのコンポーネントをアタッチする
    // 各プレイヤーからの描画命令をこのクラスが処理するのでサーバーのような役割を果たす

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

    // 消しゴムMode(まだ実装してない)
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
        // OnCollisionStayじゃないとuvを正しく算出出来なかった
        // 当たった瞬間の座標を正しく取得できていないのが気になる
        // OnCollisionEnterでは、最初の衝突はフレームの誤差の関係で正しく取得できない
        // OnCollisionStayでも同じ

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
        // 最初、スタンプにしたいテクスチャをGrahpics関数でリサイズしてから使おうとしたが、
        // リサイズしたデータはGPUメモリにした保持されていないということが発覚したのでやめた
        // (参考資料_「GetPixels()とGPUメモリ」を参照)

        // 重要なScriptを取得する
        drawing = new Drawing();
        calcUV = new CalculateUVOnCollision();

        // スタンプに使うテクスチャをDrawingに引き渡す
        Debug.Log("stampTexture_befor.isReadable = " + StampTexture.isReadable);
        drawing.StampTexture = StampTexture;
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        drawing.MeshRenderer = meshRenderer;
        drawing.CreateReFleshTexture(inputBuffer_material);
    }


    void Update()
    {
        // GUIにDebugするためのBufferを取得
        GetBuffer();
    }


#if false

    void OnGUI()
    {
        // simpleInkPainterのDebug処理
        int h = Screen.height / 3;
        int w = Screen.Width;

        GUI.DrawTexture(new Rect(0, 0 * h, h, h), input);
        GUI.DrawTexture(new Rect(0, 1 * h, h, h), inputNow);
        GUI.Box(new Rect(0, 1 * h - h / 10, h, h / 10), "input");
        GUI.Box(new Rect(0, 3 * h - h / 10, h, h / 10), "inputNow");
        kesigomu = GUI.Toggle(new Rect(w - h, 3 * h - h / 6, h, h / 6), kesigomu, "消しゴムモード");
    }

#endif
}
