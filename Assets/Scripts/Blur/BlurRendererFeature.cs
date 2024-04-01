using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TLab
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_shader = null;

        [SerializeField, Range(1f, 100f)]
        private float m_offset = 1f;

        [SerializeField, Range(10f, 1000f)]
        private float m_blur = 100f;

        [SerializeField]
        private RenderPassEvent m_renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        private GrabBluredTextureRendererPass m_grabBluredTexturePass = null;

        public override void Create()
        {
            if (m_grabBluredTexturePass == null)
            {
                m_grabBluredTexturePass = new GrabBluredTextureRendererPass(m_shader, m_renderPassEvent);
                m_grabBluredTexturePass.SetParams(m_offset, m_blur);
                m_grabBluredTexturePass.UpdateWeights();
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_grabBluredTexturePass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            m_grabBluredTexturePass.SetRenderTarget(renderer.cameraColorTargetHandle);
            m_grabBluredTexturePass.SetParams(m_offset, m_blur);
        }

        //    // Unity 2021.3.1f
        //    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        //    {
        //#if UNITY_EDITOR
        //         Debug.Log("Add Grab Blured Texture Renderer Pass.");
        //#endif
        //        m_grabBluredTexturePass.SetRenderTarget(renderer.cameraColorTarget);
        //        m_grabBluredTexturePass.SetParams(m_offset, m_blur);
        //        renderer.EnqueuePass(m_grabBluredTexturePass);
        //    }
    }
}