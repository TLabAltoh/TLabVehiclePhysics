using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class VehicleSystemManager : MonoBehaviour
    {
        public enum Pilot
        {
            Player,
            AI,
            None
        }

        [SerializeField] private VehicleEngine m_engine;
        [SerializeField] private VehicleInputManager m_inputManager;

        [SerializeField] private Pilot m_currentPilot = Pilot.Player;

        [Header("Wheels")]
        [SerializeField] private WheelColliderSource[] m_wheelColliderSources;

        public Pilot CurrentPilot { get => m_currentPilot; set => m_currentPilot = value; }

        void Start()
        {
            foreach (WheelColliderSource wheelColliderSource in m_wheelColliderSources)
            {
                wheelColliderSource.Initialize();
            }

            m_engine.Initialize();
        }

        void Update()
        {
            m_engine.UpdateShiftInput();
        }

        public void FixedUpdate()
        {
            m_engine.UpdateEngine();

            foreach (WheelColliderSource wheelColliderSource in m_wheelColliderSources)
            {
                wheelColliderSource.UpdateWheel();
            }
        }
    }
}
