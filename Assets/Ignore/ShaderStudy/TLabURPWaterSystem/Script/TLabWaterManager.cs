using UnityEngine;

[RequireComponent(typeof(TLabReflectionCameraOperator))]
public class TLabWaterManager : MonoBehaviour
{
    [SerializeField] Texture2D stampTexture;
    [SerializeField] GameObject player;
    private const int mTextureWidth = 512;
    private const int mTextureHeight = 512;
    private Texture input;
    private WaterInput waterInput;
    public Material WaveMat;            // 最終結果
    public Material WaveEquationMat;    // 波動方程式の計算
    public Material WaveNormalMat;      // 波の法線の計算
    private RenderTexture prevResult;
    private RenderTexture prev2Result;
    private RenderTexture result;
    private RenderTexture waveNormal;
    public static readonly int InputTex = Shader.PropertyToID("_InputTex");     
    public static readonly int PrevTex = Shader.PropertyToID("_PrevTex");
    public static readonly int Prev2Texv = Shader.PropertyToID("_Prev2Tex");
    public static readonly int WaveTex = Shader.PropertyToID("_WaveTex");       // Wave equation
    public static readonly int WaveNormal = Shader.PropertyToID("_WaveNormal"); // Wave normal

    public void InputInWater(Vector2 hitUv)
    {
        input = waterInput.DrawPixelStamp(hitUv);

        // 波動方程式を Shaderで GPU演算してテクスチャに反映
        WaveEquationMat.SetTexture(InputTex, input);
        WaveEquationMat.SetTexture(PrevTex, prevResult);
        WaveEquationMat.SetTexture(Prev2Texv, prev2Result);

        // 波動方程式で波を計算
        Graphics.Blit(null, result, WaveEquationMat);

        // 波の法線を計算
        WaveNormalMat.SetTexture(WaveTex, result);
        Graphics.Blit(null, waveNormal, WaveNormalMat);

        // 法線の計算結果を最終結果のマテリアルに適応する
        WaveMat.SetTexture(WaveNormal, waveNormal);

        // RenderTextureを1つずつ入れ替える
        var tmp = prev2Result;
        prev2Result = prevResult;
        prevResult = result;
        result = tmp;
    }

    void OnGUI()
    {
#if false
        // GUIでテクスチャバッファを可視化する
        int h = Screen.height / 2;
        int w = Screen.width;

        // WaveEquationMatのDebug処理(コメントアウトにされてる箇所は気になったとき自由にDebugすること)
        GUI.DrawTexture(new Rect(0, 0 * h, h, h), result);
        GUI.DrawTexture(new Rect(0, 1 * h, h, h), input);
        // GUI.DrawTexture(new Rect(w - h, 0 * h, h, h), waveNormal);
        // GUI.DrawTexture(new Rect(w - h, 1 * h, h, h), stampTexture);

        // 後述のGUI程重なった上に表示される
        // 第1,2引数 : GUIの位置. 第3,4引数 : GUIの大きさ
        const int textScale = 3;    // 0 ~ 15
        GUI.Box(new Rect(0, 1 * h - h / (15 - textScale), h, h / (15 - textScale)), "EquationResult");
        GUI.Box(new Rect(0, 2 * h - h / (15 - textScale), h, h / (15 - textScale)), "Input");
        // GUI.Box(new Rect(w - h, 1 * h - h / (15 - textScale), h, h / (15 - textScale)), "NormalResult");
        // GUI.Box(new Rect(w - h, 2 * h - h / (15 - textScale), h, h / (15 - textScale)), "StampTexture");
#endif
    }

    void Start()
    {
        if (stampTexture.isReadable == false)
        {
            Debug.LogError("stampTexture is not readable");
            return;
        }
        waterInput = new WaterInput(mTextureWidth, mTextureHeight, stampTexture);

        result = new RenderTexture(
            mTextureWidth,
            mTextureHeight,
            0,
            RenderTextureFormat.R8
        );
        prevResult = new RenderTexture(
            mTextureWidth,
            mTextureHeight,
            0,
            RenderTextureFormat.R8
        );
        prev2Result = new RenderTexture(
            mTextureWidth,
            mTextureHeight,
            0,
            RenderTextureFormat.R8
        );
        waveNormal = new RenderTexture(
            mTextureWidth,
            mTextureHeight,
            0,
            RenderTextureFormat.ARGB32
        );

        // バッファの初期化
        var r8Init = new Texture2D(1, 1);
        r8Init.SetPixel(0, 0, new Color(0.5f, 0, 0, 1));
        r8Init.Apply();
        Graphics.Blit(r8Init, prevResult);
        Graphics.Blit(r8Init, prev2Result);

        if (player)
        {
            TLabInputWithMoving manager = player.GetComponent<TLabInputWithMoving>();
            if (manager)
                manager.WaterManager = this;
            else
            {
                manager = player.AddComponent<TLabInputWithMoving>();
                manager.WaterManager = this;
            }
        }
    }
}
