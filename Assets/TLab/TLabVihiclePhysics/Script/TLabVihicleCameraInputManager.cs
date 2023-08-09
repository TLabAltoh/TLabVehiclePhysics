using System;
using UnityEngine;

public class TLabVihicleCameraInputManager : MonoBehaviour
{
    [Header("Input Config")]

    [Tooltip("Handle Input")]
    [SerializeField] string m_xInputAxis = "Horizontal_L";

    [Tooltip("Accelerator Input")]
    [SerializeField] string m_yInputAxis = "Vertical_L";

    [Tooltip("Switch Camera")]
    [SerializeField] string m_switchCameraKey = "z";

    [Header("IsMobile")]
    [SerializeField] bool m_isMobile = false;

    [Header("Audio")]
    [SerializeField] AudioSource m_switchCameraAudio;

    [Header("Vihicle System Manager")]
    [SerializeField] TLabVihicleSystemManager m_systemManager;

    [Header("UI Manager")]
    [SerializeField] TLabVihicleUIManager m_uiManager;

    [Header("Player Camera")]
    [SerializeField] TLabVihicleCamera m_playerCamera;

    private Action CameraInput;

    private void InputFromG29()
    {
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
    }

    private void InputFromKeyBoard()
    {
        m_playerCamera.SetInputFromKeyBorad(Input.GetAxis(m_xInputAxis), Input.GetAxis(m_yInputAxis));
    }

    void Start()
    {
        switch (GetComponent<TLabVihicleInputManager>().HowInput)
        {
            case (InputMode.InputFromG29):
                m_isMobile = false;
                CameraInput = InputFromG29;
                break;
            case (InputMode.InputFromKeyBord):
                m_isMobile = false;
                CameraInput = InputFromKeyBoard;
                break;
            case (InputMode.InputFromVirtualUI):
                m_isMobile = true;
                break;
        }
    }

    private void Update()
    {
        // Disable camera operation from the vehicle component when the player exits the vehicle.
        if (m_systemManager.GettingOff == true)
            return;

        if (m_isMobile) return;

        CameraInput();

        if (Input.GetKeyDown(m_switchCameraKey))
        {
            m_switchCameraAudio.Play();

            m_playerCamera.SwitchCameraMode();
            m_uiManager.OnSwitchCamera();
        }
    }
}
