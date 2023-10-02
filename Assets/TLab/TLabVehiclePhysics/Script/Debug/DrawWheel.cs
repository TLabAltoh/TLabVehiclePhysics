using UnityEngine;
using UnityEngine.Rendering;

namespace TLab.VehiclePhysics
{
    public class DrawWheel : MonoBehaviour
    {
        [SerializeField] private WheelColliderSource[] m_wheelColliderSources;

        void Start()
        {
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            foreach (WheelColliderSource wheelColliderSource in m_wheelColliderSources)
                wheelColliderSource.DrawWheel();
        }

        void OnDestroy()
        {
            RenderPipelineManager.beginCameraRendering -= OnEndCameraRendering;
        }
    }
}
