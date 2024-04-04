using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TLab
{
    public class GrabBluredTextureRendererPass : ScriptableRenderPass
    {
        // link: https://edom18.hateblo.jp/entry/2020/11/02/080719

        private const string NAME = nameof(GrabBluredTextureRendererPass);

        private Material m_material = null;
        private RenderTargetIdentifier m_currentTarget = default;
        private float m_offset = 0;
        private float m_blur = 0;

        private float[] m_weights = new float[10];

        private int m_blurredTempID1 = 0;
        private int m_blurredTempID2 = 0;
        private int m_screenCopyID = 0;
        private int m_weightsID = 0;
        private int m_offsetsID = 0;
        private int m_grabBlurTextureID = 0;

        public GrabBluredTextureRendererPass(Shader shader, RenderPassEvent passEvent)
        {
            renderPassEvent = passEvent;
            m_material = new Material(shader);

            m_blurredTempID1 = Shader.PropertyToID("_BlurTemp1");
            m_blurredTempID2 = Shader.PropertyToID("_BlurTemp2");
            m_screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
            m_weightsID = Shader.PropertyToID("_Weights");
            m_offsetsID = Shader.PropertyToID("_Offsets");
            m_grabBlurTextureID = Shader.PropertyToID("_GrabBlurTexture");
        }

        public void UpdateWeights()
        {
            float total = 0;
            float d = m_blur * m_blur * 0.001f;

            for (int i = 0; i < m_weights.Length; i++)
            {
                float r = 1.0f + 2.0f * i;
                float w = Mathf.Exp(-0.5f * (r * r) / d);
                m_weights[i] = w;
                if (i > 0) w *= 2.0f;

                total += w;
            }

            for (int i = 0; i < m_weights.Length; i++)
                m_weights[i] /= total;
        }

        public void SetParams(float offset, float blur)
        {
            m_offset = offset;
            m_blur = blur;
        }

        public void SetRenderTarget(RenderTargetIdentifier target)
        {
            m_currentTarget = target;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera) return;

            CommandBuffer buf = CommandBufferPool.Get(NAME);

            ref CameraData camData = ref renderingData.cameraData;

            if (camData.isSceneViewCamera) return;

            RenderTextureDescriptor descriptor = camData.cameraTargetDescriptor;

            buf.GetTemporaryRT(m_screenCopyID, descriptor, FilterMode.Bilinear);

            descriptor.width /= 2;
            descriptor.height /= 2;

            buf.GetTemporaryRT(m_blurredTempID1, descriptor, FilterMode.Bilinear);
            buf.GetTemporaryRT(m_blurredTempID2, descriptor, FilterMode.Bilinear);

            int width = camData.camera.scaledPixelWidth;
            int height = camData.camera.scaledPixelHeight;
            float x = m_offset / width;
            float y = m_offset / height;

            buf.SetGlobalFloatArray(m_weightsID, m_weights);

            buf.Blit(m_currentTarget, m_screenCopyID);

            buf.Blit(m_screenCopyID, m_blurredTempID1);
            buf.ReleaseTemporaryRT(m_screenCopyID);

            buf.SetGlobalVector(m_offsetsID, new Vector4(x, 0, 0, 0));
            buf.Blit(m_blurredTempID1, m_blurredTempID2, m_material);

            buf.SetGlobalVector(m_offsetsID, new Vector4(0, y, 0, 0));
            buf.Blit(m_blurredTempID2, m_blurredTempID1, m_material);

            buf.ReleaseTemporaryRT(m_blurredTempID2);

            buf.SetGlobalTexture(m_grabBlurTextureID, m_blurredTempID1);

            context.ExecuteCommandBuffer(buf);
            CommandBufferPool.Release(buf);
        }
    }
}