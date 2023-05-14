using UnityEngine;

public class WaterInput
{
    private Texture2D Input;
    private Texture2D mStampTexture;
    private Color[] stampBuffer;
    private Color[] colorBuffer;
    private Color[] refleshColorBuffer;
    private Vector2 prevHit;
    private int mTexWidth;
    private int mTexHeight;
    private int mStampTexWidth;
    private int mStampTexHalfWidth;
    private int mStampTexHeight;
    private int mStampTexHalfHeight;

    public WaterInput(int textureWidth, int textureHeight, Texture2D stampTexture)
    {
        mTexWidth = textureWidth;
        mTexHeight = textureHeight;
        mStampTexture = stampTexture;
        mStampTexWidth = stampTexture.width;
        mStampTexHeight = stampTexture.height;
        mStampTexHalfWidth = stampTexture.width / 2;
        mStampTexHalfHeight = stampTexture.height / 2;
        stampBuffer = stampTexture.GetPixels();

        Input = new Texture2D(mTexWidth, mTexHeight, TextureFormat.RGBA32, false);
        Input.filterMode = FilterMode.Point;

        refleshColorBuffer = new Color[mTexWidth * mTexHeight];
        for (int x = 0; x < mTexWidth; x++)
            for (int y = 0; y < mTexHeight; y++)
                refleshColorBuffer[x + mTexWidth * y] = new Color(0, 0, 0, 0);

        colorBuffer = new Color[refleshColorBuffer.Length];
        refleshColorBuffer.CopyTo(colorBuffer, 0);

        Input.SetPixels(colorBuffer);
        Input.Apply();

        prevHit = Vector2.zero;
    }

    public Texture DrawPixelStamp(Vector2 p)
    {
        refleshColorBuffer.CopyTo(colorBuffer, 0);

        if (prevHit.x != p.x && p.x != 0 && prevHit.y != p.y && p.y != 0)
        {
            prevHit = p;

            int inputX = (int)(p.x * Input.width);
            int inputY = (int)(p.y * Input.height);
            int bufferXStart = inputX - mStampTexHalfWidth;
            int bufferXEnd = inputX + mStampTexHalfWidth;
            int bufferYStart = inputY - mStampTexHalfHeight;
            int bufferYEnd = inputY + mStampTexHalfHeight;

            int stampXStart = 0;
            int stampX = 0;
            int stampY = 0;

            if (bufferXStart < 0)
            {
                int over = 0 - bufferXStart;
                stampXStart = stampXStart + over;
                bufferXStart = 0;
            }
            if (bufferXEnd > Input.width)
                bufferXEnd = Input.width;
            if (bufferYStart < 0)
            {
                int over = 0 - bufferYStart;
                stampY = stampY + over;
                bufferYStart = 0;
            }
            if (bufferYEnd > Input.height)
                bufferYEnd = Input.height;

#if false
            Debug.Log(
                "inputX: " + inputX.ToString() + ", " +
                "inputY: " + inputY.ToString() + ", " +
                "bufferXStart: " + bufferXStart.ToString() + ", " +
                "bufferXEnd: " + bufferXEnd.ToString() + ", " +
                "bufferYStart: " + bufferYStart.ToString() + ", " +
                "bufferYEnd: " + bufferYEnd.ToString()
            );
#endif

            for (int y = bufferYStart; y < bufferYEnd; y++)
            {
                stampX = stampXStart;
                int bufferLineOffset = y * Input.height;
                int stampLineOffset = (stampY++) * mStampTexWidth;
                for (int x = bufferXStart; x < bufferXEnd; x++)
                    colorBuffer[x + bufferLineOffset] = stampBuffer[(stampX++) + stampLineOffset] * 100;
            }
        }
        Input.SetPixels(colorBuffer);
        Input.Apply();

        return Input;
    }
}
