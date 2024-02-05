using UnityEngine;
using UnityEngine.Events;
using TLab;
using TLab.VehiclePhysics;

public class VehicleCameraInputManager : MonoBehaviour
{
    [SerializeField] private VirtualInputAxis m_xInputAxis;
    [SerializeField] private VirtualInputAxis m_yInputAxis;
    [SerializeField] private KeyCode m_switchCameraKey;

    [Space(10)]
    [SerializeField] private VehicleSystemManager m_systemManager;
    [SerializeField] private VehicleInputManager m_inputManager;
    [SerializeField] private VehicleCamera m_playerCamera;

    [Space(10)]
    [SerializeField] private UnityEvent m_onCameraSwitch;

    private void GetSwitchCameraEvent()
    {
        if (Input.GetKeyDown(m_switchCameraKey))
        {
            m_playerCamera.SwitchMode();
            m_onCameraSwitch.Invoke();
        }
    }

    private void PilotIsPlayer()
    {
        switch (m_inputManager.howInput)
        {
            case VehicleInputManager.InputMode.G29:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);

                    switch (rec.rgdwPOV[0])
                    {
                        case (0): m_playerCamera.SetInputFromKeyBorad(0f, -1f); break;
                        case (4500): m_playerCamera.SetInputFromKeyBorad(1f, -1f); break;
                        case (9000): m_playerCamera.SetInputFromKeyBorad(1f, 0f); break;
                        case (13500): m_playerCamera.SetInputFromKeyBorad(1f, 1f); break;
                        case (18000): m_playerCamera.SetInputFromKeyBorad(0f, 1f); break;
                        case (22500): m_playerCamera.SetInputFromKeyBorad(-1f, 1f); break;
                        case (27000): m_playerCamera.SetInputFromKeyBorad(-1f, 0f); break;
                        case (31500): m_playerCamera.SetInputFromKeyBorad(-1f, -1f); break;
                        default: m_playerCamera.SetInputFromKeyBorad(0, 0); break;
                    }
                }
                GetSwitchCameraEvent();
                break;
            case VehicleInputManager.InputMode.MOUSE:
                GetSwitchCameraEvent();
                break;
            case VehicleInputManager.InputMode.KEYBORAD:
                m_playerCamera.SetInputFromKeyBorad(m_xInputAxis.AxisValue, m_yInputAxis.AxisValue);
                GetSwitchCameraEvent();
                break;
            case VehicleInputManager.InputMode.UI_BUTTON:
                break;
        }
    }

    private void InitializeWithInputMode()
    {
        switch (GetComponent<VehicleInputManager>().howInput)
        {
            case VehicleInputManager.InputMode.G29:
                break;
            case VehicleInputManager.InputMode.MOUSE:
                break;
            case VehicleInputManager.InputMode.KEYBORAD:
                break;
            case VehicleInputManager.InputMode.UI_BUTTON:
                break;
        }
    }

    private void InitializeWithPilot()
    {
        switch (m_systemManager.CurrentPilot)
        {
            case VehicleSystemManager.Pilot.None:
                break;
            case VehicleSystemManager.Pilot.AI:
                break;
            case VehicleSystemManager.Pilot.Player:
                InitializeWithInputMode();
                break;
        }
    }

    void Start()
    {
        InitializeWithPilot();
    }

    private void Update()
    {
        switch (m_systemManager.CurrentPilot)
        {
            case VehicleSystemManager.Pilot.None:
                break;
            case VehicleSystemManager.Pilot.AI:
                break;
            case VehicleSystemManager.Pilot.Player:
                PilotIsPlayer();
                break;
        }
    }
}
