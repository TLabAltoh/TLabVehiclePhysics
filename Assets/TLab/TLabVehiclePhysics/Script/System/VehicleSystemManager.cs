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

        [Header("Initial")]
        [SerializeField] private bool m_shiftEnabled = true;
        [SerializeField] private int m_initialEngineGear = 2;
        [SerializeField] VehicleEngine.State m_initialEngineState = VehicleEngine.State.ON;

        [Header("Wheels")]
        [SerializeField] private WheelColliderSource[] m_wheelColliderSources;

        public Pilot CurrentPilot { get => m_currentPilot; set => m_currentPilot = value; }

        public void SetShiftEnabled(bool enabled)
        {
            m_shiftEnabled = enabled;
        }

        void Start()
        {
            foreach (WheelColliderSource wheelColliderSource in m_wheelColliderSources)
            {
                wheelColliderSource.Initialize();
            }

            m_engine.Initialize(m_initialEngineState, m_initialEngineGear);
        }

        void Update()
        {
            if (m_shiftEnabled)
            {
                m_engine.UpdateShiftInput();
            }
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
