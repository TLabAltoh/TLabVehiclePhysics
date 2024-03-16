using UnityEngine;
using UnityEngine.Rendering;

namespace TLab.VehiclePhysics
{
    public class DrawWheel : MonoBehaviour
    {
        [SerializeField] private WheelColliderSource[] m_wheelColliderSources;

        private void Start()
        {
            RenderPipelineManager.endCameraRendering += OnRenderEvent;
        }

        private void OnRenderEvent(ScriptableRenderContext context, Camera camera)
        {
            GL.Clear(true, false, Color.black);

            foreach (WheelColliderSource wheelColliderSource in m_wheelColliderSources)
            {
                wheelColliderSource.DrawWheel();
            }
        }

        private void OnDestroy()
        {
            RenderPipelineManager.endCameraRendering -= OnRenderEvent;
        }
    }
}
