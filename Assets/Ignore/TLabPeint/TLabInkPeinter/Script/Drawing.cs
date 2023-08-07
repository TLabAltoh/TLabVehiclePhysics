using UnityEngine;

public class Drawing
{
    #region Propaty

    // drawInputNow�� reflesh����ׂ� Texture
    private Texture2D refleshTexture;
    private Texture2D Input;
    private Texture2D InputNow;
    private Texture2D stampTexture;
    private Color[] colorBuffer;
    private Color[] colorBufferNow;
    private Color[] pixels;
    private Color[] reflesh;

    // �X�^���v�Ɏg���J���[�o�b�t�@
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

    // �ړI��Texture��ێ����Ă���Material(inkPainter1����q�؂���)
    private Material inputBuffer_material;

    // �V�F�[�_�[�O���t�̃v���p�e�B��Reference�ɐݒ肵���l�ƂȂ�
    // �y�C���g�̍��v���ʂ̃e�N�X�`��
    private readonly int ShaderPropertyInput = Shader.PropertyToID("input");

    // �����_�̏u�ԂɃC���v�b�g�����s�N�Z���������_�����O����e�N�X�`��
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


    #region colorBufferNow��������


    public void UpdateBufferNowTexture()
    {
        // drawInputNow�𖈃t���[������������
        reflesh.CopyTo(colorBufferNow, 0);
        InputNow.SetPixels(colorBufferNow);
        InputNow.Apply();

        // ���񃌃��_�����O�����s�N�Z����Material�Ɉ����n��
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion colorBufferNow��������


    #region Mouse, laser, FromCar�ł̃y�C���g����(�X�^���v�y�C���g)


    public Color[] DrawStamp(Vector2 p, Color[] Buffer)
    {
        // �X�^���v�e�N�X�`�����J���[�o�b�t�@�ɏ�������
        // SetValue(Color, Index)�͏d�������Ȃ̂Ŏg�p�������

        // for���̒��ł�if���͂߂���߂��Ꮘ�����d���Ȃ�̂Ŕ�����(15fps�܂ŏ�����������)
        int x, y, z, w;

        // �X�^���v�e�N�X�`���̉E�������y�C���g
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

        //�X�^���v�e�N�X�`���̍��������y�C���g
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
        // �J���[�o�b�t�@���X�V���ăV�F�[�_�[�̃e�N�X�`���ɔ��f����

        // InputBufferNow�����͖�flame�܂�����ɍX�V���Ȃ��Ƃ����Ȃ��̂ŁC��������s����
        UpdateBufferNowTexture();

        // DrawStamp�����s

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
        /*����Object���g������Material��Texture��Input_buffer��Material������Texture*/
        /*�̌v3��Texture�ɕ`�挋�ʂ𔽉f����                                      */
        /****************************************************************************/

        // �X�N���v�g����Input_buffer��Material�Ƀe�N�X�`�������蓖�Ă�
        // ��1���� : �v���p�e�B��, ��2���� : ���蓖�Ă�e�N�X�`����
        inputBuffer_material.SetTexture(ShaderPropertyInput, Input);
        
        // ���񃌃��_�����O�����s�N�Z����\��
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion Mouse, laser, FromCar�ł̃y�C���g����(�X�^���v�y�C���g)


    #region Collision�ł̃y�C���g����(�h�b�g�y�C���g)


    public Color[] Draw(Vector2 p, Color[] Buffer, Color color)
    {
        // �s�N�Z���̂ǂ̈ʒu((int)p.x + 256 * (int)p.y)�ɍ��F��z�u���邩
        Buffer[(int)p.x + 256 * (int)p.y] = color;
        return Buffer;
    }


    public void DrawPixel(Vector2 p)
    {
        // ������ uv���W����h�b�g�P�ʂŃJ���[�o�b�t�@���X�V

        // InputBufferNow�����͖�flame�܂�����ɍX�V���Ȃ��Ƃ����Ȃ��̂ŁC��������s����
        UpdateBufferNowTexture();

        // public Color[] Draw�����s����

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
        /*����Object���g������Material��Texture��Input_buffer��Material������Texture*/
        /*�̌v3��Texture�ɕ`�挋�ʂ𔽉f����                                      */
        /****************************************************************************/

        //�X�N���v�g����Input_buffer��Material�Ƀe�N�X�`�������蓖�Ă�
        //��1���� : �v���p�e�B��, ��2���� : ���蓖�Ă�e�N�X�`����
        inputBuffer_material.SetTexture(ShaderPropertyInput, Input);
        //���񃌃��_�����O�����s�N�Z����\��
        inputBuffer_material.SetTexture(ShaderPropertyInputNow, InputNow);
    }


    #endregion Collision�ɂ��y�C���g����(�h�b�g�y�C���g)


    #region �L�����o�X�̏�����


    public void SetColorBuffer_and_Texture(Material inputBufferMat)
    {
        Input = new Texture2D(refleshTexture.width, refleshTexture.height, TextureFormat.RGBA32, false);

        // �h�b�g���t�B���^�[�����ɕ`�悷��
        Input.filterMode = FilterMode.Point;
        InputNow = new Texture2D(refleshTexture.width, refleshTexture.height, TextureFormat.RGBA32, false);
        InputNow.filterMode = FilterMode.Point;

        // pixels.Length : ���s�N�Z����
        colorBuffer = new Color[pixels.Length];
        colorBufferNow = new Color[reflesh.Length];
        Debug.Log("stampTexture.isReadable = " + stampTexture.isReadable);
        stampBuffer = stampTexture.GetPixels();

        // buffer�Ɍ��̃e�N�X�`���̃s�N�Z�����R�s�[����
        // 0�͔z��̊J�n�C���f�b�N�X
        pixels.CopyTo(colorBuffer, 0);
        reflesh.CopyTo(colorBufferNow, 0);

        // �e�N�X�`�������炩���߃L�����o�X�̐F�ɂ���
        Input.SetPixels(pixels);
        Input.Apply();
        InputNow.SetPixels(reflesh);
        InputNow.Apply();

        // �ړI��Texture�������Ă���Material���擾
        inputBuffer_material = inputBufferMat;
    }


    public void CreateReFleshTexture(Material inputBufferMat)
    {
        // �L�����o�X�̃o�b�t�@�����t���b�V������ׂ̃e�N�X�`���𐶐�

        pixels = new Color[512 * 512];
        refleshTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        Debug.Log("texture.isReadable = " + refleshTexture.isReadable);

        for (int x = 0; x < 512; x++)
        {
            for (int y = 0; y < 512; y++)
            {
                // �L�����o�X�����̐F�Ń��t���b�V��
                pixels[x + 512 * y] = new Color(1, 1, 1, 1);
            }
        }
        refleshTexture.SetPixels(pixels);
        refleshTexture.Apply();

        // colorBufferNow�͂���Ŗ��񃊃t���b�V������
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


    #endregion �L�����o�X�̏�����
}
