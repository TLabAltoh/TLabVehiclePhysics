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
    public Material WaveMat;            // �ŏI����
    public Material WaveEquationMat;    // �g���������̌v�Z
    public Material WaveNormalMat;      // �g�̖@���̌v�Z
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

        // �g���������� Shader�� GPU���Z���ăe�N�X�`���ɔ��f
        WaveEquationMat.SetTexture(InputTex, input);
        WaveEquationMat.SetTexture(PrevTex, prevResult);
        WaveEquationMat.SetTexture(Prev2Texv, prev2Result);

        // �g���������Ŕg���v�Z
        Graphics.Blit(null, result, WaveEquationMat);

        // �g�̖@�����v�Z
        WaveNormalMat.SetTexture(WaveTex, result);
        Graphics.Blit(null, waveNormal, WaveNormalMat);

        // �@���̌v�Z���ʂ��ŏI���ʂ̃}�e���A���ɓK������
        WaveMat.SetTexture(WaveNormal, waveNormal);

        // RenderTexture��1������ւ���
        var tmp = prev2Result;
        prev2Result = prevResult;
        prevResult = result;
        result = tmp;
    }

    void OnGUI()
    {
#if false
        // GUI�Ńe�N�X�`���o�b�t�@����������
        int h = Screen.height / 2;
        int w = Screen.width;

        // WaveEquationMat��Debug����(�R�����g�A�E�g�ɂ���Ă�ӏ��͋C�ɂȂ����Ƃ����R��Debug���邱��)
        GUI.DrawTexture(new Rect(0, 0 * h, h, h), result);
        GUI.DrawTexture(new Rect(0, 1 * h, h, h), input);
        // GUI.DrawTexture(new Rect(w - h, 0 * h, h, h), waveNormal);
        // GUI.DrawTexture(new Rect(w - h, 1 * h, h, h), stampTexture);

        // ��q��GUI���d�Ȃ�����ɕ\�������
        // ��1,2���� : GUI�̈ʒu. ��3,4���� : GUI�̑傫��
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

        // �o�b�t�@�̏�����
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
