using UnityEngine;

public class Drawing
{
    #region Propaty

    // drawInputNowを refleshする為の Texture
    private Texture2D refleshTexture;
    private Texture2D Input;
    private Texture2D InputNow;
    private Texture2D stampTexture;
    private Color[] colorBuffer;
    private Color[] colorBufferNow;
    private Color[] pixels;
    private Color[] reflesh;

    // スタンプに使うカラーバッファ
    private Color[] stampBuffer;
    private MeshRenderer meshRenderer;
    private bool WhichDrawOrNot;

    private Vector2 prevHit = new Vector2(0, 0);
    private bool hitSame;

    public Texture2D StampTexture
    {
        get
        {
            return stampTexture;
        }
        set
        {
            stampTexture = value;
        }
    }

    public MeshRenderer MeshRenderer
    {
        get
        {
            return meshRenderer;
        }
        set
        {
            meshRenderer = value;
        }
    }

    // 目的のTextureを保持しているMaterial(inkPainter1から拝借する)
    private Material inputBuffer_material;

    // シェーダーグラフのプロパティはReferenceに設定した値となる
    // ペイントの合計結果のテクスチャ
    private readonly int ShaderPropertyInput = Shader.PropertyToID("input");

    // 現時点の瞬間にインプットしたピクセルをレンダリングするテクスチャ
    private readonly int ShaderPropertyInputNow = Shader.PropertyToID("input_now");

    public Material InputBufferMaterial
    {
        get
        {
            return inputBuffer_material;
        }
        set
        {
            inputBuffer_material = value;
        }
    }

    #endregion Propaty


    #region colorBufferNowを初期化


    public void UpdateBufferNowTexture()
    {
        // drawInputNowを毎フレーム初期化する
        reflesh.CopyTo(colorBufferNow, 0);
        InputNow.SetPixels(colorBufferNow);
        InputNow.Apply();

        // 今回レンダリングしたピクセルをMaterialに引き渡す
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion colorBufferNowを初期化


    #region Mouse, laser, FromCarでのペイント処理(スタンプペイント)


    public Color[] DrawStamp(Vector2 p, Color[] Buffer)
    {
        // スタンプテクスチャをカラーバッファに書き込む
        // SetValue(Color, Index)は重い処理なので使用を避ける

        // for文の中でのif文はめちゃめちゃ処理が重くなるので避ける(15fpsまで処理が落ちた)
        int x, y, z, w;

        // スタンプテクスチャの右半分をペイント
        z = 32;
        for (x = (int)p.x; x < (int)(p.x + 32) && x < Input.width; x++)
        {
            w = 32;
            for (y = (int)p.y; y < (int)(p.y + 32) && y < Input.height; y++)
            {
                Buffer[x + Input.height * y] = stampBuffer[z + 64 * w];
                w++;
            }
            w = 32;
            for (y = (int)p.y; y > (int)(p.y - 32) && y > 0; y--)
            {
                Buffer[x + Input.height * y] = stampBuffer[z + 64 * w];
                w--;
            }
            z++;
        }

        //スタンプテクスチャの左半分をペイント
        z = 32;
        for (x = (int)p.x; x > (int)(p.x - 32) && x > 0; x--)
        {
            w = 32;
            for (y = (int)p.y; y < (int)(p.y + 32) && y < Input.height; y++)
            {
                Buffer[x + Input.height * y] = stampBuffer[z + 64 * w];
                w++;
            }
            w = 32;
            for (y = (int)p.y; y > (int)(p.y - 32) && y > 0; y--)
            {
                Buffer[x + Input.height * y] = stampBuffer[z + 64 * w];
                w--;
            }
            z--;
        }

        return Buffer;
    }


    public void DrawPixelStamp(Vector2 p)
    {
        // カラーバッファを更新してシェーダーのテクスチャに反映する

        // InputBufferNowだけは毎flameまっさらに更新しないといけないので，これを実行する
        UpdateBufferNowTexture();

        // DrawStampを実行

        if (prevHit.x != p.x || prevHit.y != p.y)
        {
            colorBuffer = DrawStamp(p * Input.width, colorBuffer);
            colorBufferNow = DrawStamp(p * Input.width, colorBufferNow);

            Input.SetPixels(colorBuffer);
            Input.Apply();
            InputNow.SetPixels(colorBufferNow);
            InputNow.Apply();

            prevHit = p;
        }

        /****************************************************************************/
        /*このObject自身が持つMaterialのTextureとInput_bufferのMaterialが持つTexture*/
        /*の計3つのTextureに描画結果を反映する                                      */
        /****************************************************************************/

        // スクリプトからInput_bufferのMaterialにテクスチャを割り当てる
        // 第1引数 : プロパティ名, 第2引数 : 割り当てるテクスチャ名
        inputBuffer_material.SetTexture(ShaderPropertyInput, Input);
        
        // 今回レンダリングしたピクセルを表示
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion Mouse, laser, FromCarでのペイント処理(スタンプペイント)


    #region Collisionでのペイント処理(ドットペイント)


    public Color[] Draw(Vector2 p, Color[] Buffer, Color color)
    {
        // ピクセルのどの位置((int)p.x + 256 * (int)p.y)に黒色を配置するか
        Buffer[(int)p.x + 256 * (int)p.y] = color;
        return Buffer;
    }


    public void DrawPixel(Vector2 p)
    {
        // 引数の uv座標からドット単位でカラーバッファを更新

        // InputBufferNowだけは毎flameまっさらに更新しないといけないので，これを実行する
        UpdateBufferNowTexture();

        // public Color[] Drawを実行する

        if (prevHit.x != p.x || prevHit.y != p.y)
        {
            colorBuffer = Draw(p * 256, colorBuffer, new Color(0f, 0f, 0f, 1));
            colorBufferNow = Draw(p * 256, colorBufferNow, new Color(0f, 0f, 0f, 1));

            Input.SetPixels(colorBuffer);
            Input.Apply();
            InputNow.SetPixels(colorBufferNow);
            InputNow.Apply();

            prevHit = p;
        }

        /****************************************************************************/
        /*このObject自身が持つMaterialのTextureとInput_bufferのMaterialが持つTexture*/
        /*の計3つのTextureに描画結果を反映する                                      */
        /****************************************************************************/

        //スクリプトからInput_bufferのMaterialにテクスチャを割り当てる
        //第1引数 : プロパティ名, 第2引数 : 割り当てるテクスチャ名
        inputBuffer_material.SetTexture(ShaderPropertyInput, Input);
        //今回レンダリングしたピクセルを表示
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion Collisionによるペイント処理(ドットペイント)


    #region キャンバスの初期化


    public void SetColorBuffer_and_Texture(Material inputBufferMat)
    {
        Input = new Texture2D(refleshTexture.width, refleshTexture.height, TextureFormat.RGBA32, false);

        // ドットをフィルターせずに描画する
        Input.filterMode = FilterMode.Point;
        InputNow = new Texture2D(refleshTexture.width, refleshTexture.height, TextureFormat.RGBA32, false);
        InputNow.filterMode = FilterMode.Point;

        // pixels.Length : 総ピクセル数
        colorBuffer = new Color[pixels.Length];
        colorBufferNow = new Color[reflesh.Length];
        Debug.Log("stampTexture.isReadable = " + stampTexture.isReadable);
        stampBuffer = stampTexture.GetPixels();

        // bufferに元のテクスチャのピクセルをコピーする
        // 0は配列の開始インデックス
        pixels.CopyTo(colorBuffer, 0);
        reflesh.CopyTo(colorBufferNow, 0);

        // テクスチャをあらかじめキャンバスの色にする
        Input.SetPixels(pixels);
        Input.Apply();
        InputNow.SetPixels(reflesh);
        InputNow.Apply();

        // 目的のTextureを持っているMaterialを取得
        inputBuffer_material = inputBufferMat;
    }


    public void CreateReFleshTexture(Material inputBufferMat)
    {
        // キャンバスのバッファをリフレッシュする為のテクスチャを生成

        pixels = new Color[512 * 512];
        refleshTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        Debug.Log("texture.isReadable = " + refleshTexture.isReadable);

        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                // キャンバスをこの色でリフレッシュ
                pixels[x + 512 * y] = new Color(1, 1, 1, 1);
            }
        }
        refleshTexture.SetPixels(pixels);
        refleshTexture.Apply();

        // colorBufferNowはこれで毎回リフレッシュする
        reflesh = new Color[512 * 512];
        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                reflesh[x + 512 * y] = new Color(0, 0, 0, 0);
            }
        }

        SetColorBuffer_and_Texture(inputBufferMat);
    }


    #endregion キャンバスの初期化
}
