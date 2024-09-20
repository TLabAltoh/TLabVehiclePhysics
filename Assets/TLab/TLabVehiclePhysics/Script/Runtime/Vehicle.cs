using UnityEngine;
using TLab.VehiclePhysics.Input;

namespace TLab.VehiclePhysics
{
    public class Vehicle : MonoBehaviour
    {
        public enum Pilot
        {
            Player,
            AI,
            None
        }

        [Header("Input Settings")]
        [SerializeField] private InputManager m_inputManager;
        [SerializeField] private Pilot m_currentPilot = Pilot.Player;

        [Header("Engine Settings")]
        [SerializeField] private Engine m_engine;
        [SerializeField] private bool m_shiftEnabled = true;
        [SerializeField] private int m_initialEngineGear = 2;
        [SerializeField] Engine.State m_initialEngineState = Engine.State.ON;

        [Header("Wheel Settings")]
        [SerializeField] private WheelColliderSource[] m_wheels;

        public Pilot CurrentPilot { get => m_currentPilot; set => m_currentPilot = value; }

        public void SetShiftEnabled(bool enabled)
        {
            m_shiftEnabled = enabled;
        }

        void Start()
        {
            foreach (var wheel in m_wheels)
                wheel.Initialize();

            m_engine.Initialize(m_initialEngineState, m_initialEngineGear);
        }

        void Update()
        {
            if (m_shiftEnabled)
                m_engine.UpdateShiftInput();
        }

        public void FixedUpdate()
        {
            m_engine.UpdateEngine();

            foreach (var wheel in m_wheels)
                wheel.UpdateWheel();
        }
    }
}
