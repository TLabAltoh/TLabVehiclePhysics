using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    [SerializeField]
    private Shader _shader = null;

    [SerializeField, Range(1f, 100f)]
    private float _offset = 1f;

    [SerializeField, Range(10f, 1000f)]
    private float _blur = 100f;

    [SerializeField]
    private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    private GrabBluredTextureRendererPass _grabBluredTexturePass = null;

    public override void Create()
    {
#if UNITY_EDITOR
        // Debug.Log("Create Blur Renderer Feature.");
#endif
        if (_grabBluredTexturePass == null)
        {
            _grabBluredTexturePass = new GrabBluredTextureRendererPass(_shader, _renderPassEvent);
            _grabBluredTexturePass.SetParams(_offset, _blur);
            _grabBluredTexturePass.UpdateWeights();
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        // Debug.Log("Add Grab Blured Texture Renderer Pass.");
#endif
        _grabBluredTexturePass.SetRenderTarget(renderer.cameraColorTarget);
        _grabBluredTexturePass.SetParams(_offset, _blur);
        renderer.EnqueuePass(_grabBluredTexturePass);
    }
}