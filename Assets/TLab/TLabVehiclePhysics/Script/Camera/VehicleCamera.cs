using System.Collections;
using UnityEngine;
using TLab.VehiclePhysics;

public class VehicleCamera : MonoBehaviour
{
    [System.Serializable]
    public class ShakeSetting
    {
        public float min;
        public float max;
        public float lerp;
    }

    public enum Mode
    {
        FirstPerson,
        TherdPerson
    };

    [SerializeField] private Camera m_camera;
    [SerializeField] private Mode m_cameraMode;

    [Header("FPS Settings")]
    [SerializeField] private ShakeSetting m_shakeX;
    [SerializeField] private ShakeSetting m_shakeY;
    [SerializeField] private ShakeSetting m_shakeZ;
    [Space(10)]
    [SerializeField] private int m_maxVertical = 60;
    [SerializeField] private int m_maxHorizontal = 60;

    [Header("TPS Settings")]
    [SerializeField] private float m_cameraDistance = 3.2f;
    [SerializeField] private float m_cameraHeight = 0.8f;

    [SerializeField] private Rigidbody m_rigidbody;

    [SerializeField] private VehicleSystemManager m_systemManager;

    private float m_xInput;
    private float m_yInput;

    private Transform m_lookObj;
    private Vector3 m_lookDir;
    private Vector3 m_forwardLook;
    private Vector3 m_upLook;
    private float m_actualVertical;
    private float m_actualHorizontal;
    private float m_smoothYRot;

    private Vector3 m_velocity;
    private Vector3 m_prevVelocity;
    private Vector3 m_cameraShake;
    private Vector3 m_cameraOffset;

    private Vector3 LocalVelocity => this.transform.InverseTransformDirection(m_rigidbody.velocity);
    private Vector3 VelocityDelta => m_velocity - m_prevVelocity;

    public void SetInputFromKeyBorad(float x, float y)
    {
        m_xInput = x;
        m_yInput = y;
    }

    public void SwitchMode()
    {
        switch (m_cameraMode)
        {
            case Mode.FirstPerson:
                m_cameraMode = Mode.TherdPerson;
                m_forwardLook = transform.forward;
                break;
            case Mode.TherdPerson:
                m_cameraMode = Mode.FirstPerson;
                break;
        }
    }

    private void PilotTrackable()
    {
        switch (m_cameraMode)
        {
            case Mode.FirstPerson:
                // update camera position
#if true
                m_cameraShake.x = Mathf.Lerp(m_cameraShake.x, VelocityDelta.x, m_shakeX.lerp);
                m_cameraShake.y = Mathf.Lerp(m_cameraShake.y, VelocityDelta.y, m_shakeY.lerp);
                m_cameraShake.x = Mathf.Clamp(m_cameraShake.x, m_shakeX.min, m_shakeX.max);
                m_cameraShake.y = Mathf.Clamp(m_cameraShake.y, m_shakeY.min, m_shakeY.max);
                m_camera.transform.position = transform.TransformPoint(m_cameraOffset - m_cameraShake);
#else
                m_camera.transform.position = transform.TransformPoint(m_cameraOffset);
#endif
                // ç°ÇÃÇ∆Ç±ÇÎÇ±ÇÍÇ™å¿äE!
                float fieldOfViewBase = 60.0f;
                m_camera.fieldOfView = fieldOfViewBase + m_velocity.z;

                // update camera rotation
                m_actualVertical = Mathf.Lerp(m_actualVertical, m_maxVertical * m_yInput, 0.1f);
                m_actualHorizontal = Mathf.Lerp(m_actualHorizontal, m_maxHorizontal * m_xInput, 0.1f);
                Quaternion quaternion = transform.rotation;
                m_camera.transform.rotation = Quaternion.Euler(quaternion.eulerAngles.x + m_actualVertical, quaternion.eulerAngles.y + m_actualHorizontal, quaternion.eulerAngles.z);
                break;
            case Mode.TherdPerson:
                m_lookDir = Vector3.Slerp(m_lookDir, (m_xInput == 0 && m_yInput == 0 ? Vector3.forward : new Vector3(m_xInput, 0, m_yInput).normalized), 0.1f);
                m_smoothYRot = Mathf.Lerp(m_smoothYRot, m_rigidbody.angularVelocity.y, 0.02f);

                m_forwardLook = Vector3.Lerp(m_forwardLook, transform.forward, 0.05f);
                m_upLook = Vector3.Lerp(m_upLook, transform.up, 0.05f);

                m_lookObj.rotation = Quaternion.LookRotation(m_forwardLook, m_upLook);
                m_lookObj.position = transform.position;

                Vector3 lookDirActual = (m_lookDir - new Vector3(Mathf.Sin(m_smoothYRot), 0, Mathf.Cos(m_smoothYRot)) * Mathf.Abs(m_smoothYRot) * 0.2f).normalized;
                Vector3 forwardDir = m_lookObj.TransformDirection(lookDirActual);
                Vector3 localOffset = m_lookObj.TransformPoint(-lookDirActual * m_cameraDistance - lookDirActual * Mathf.Min(m_rigidbody.velocity.magnitude * 0.05f, 2) + Vector3.up * m_cameraHeight);

                m_camera.transform.position = localOffset;
                m_camera.transform.rotation = Quaternion.LookRotation(forwardDir, m_lookObj.up);
                break;
        }
    }

    IEnumerator UpdateCamera()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            m_prevVelocity = m_velocity;
            m_velocity = LocalVelocity;

            switch (m_systemManager.CurrentPilot)
            {
                case VehicleSystemManager.Pilot.None:
                    //
                    break;
                case VehicleSystemManager.Pilot.AI:
                    //
                    break;
                case VehicleSystemManager.Pilot.Player:
                    PilotTrackable();
                    break;
            }
        }
    }

    void Start()
    {
        m_camera.transform.parent = null;
        m_lookObj = new GameObject()
        {
            name = "Camera Looker",
            hideFlags = HideFlags.HideInHierarchy
        }.transform;
        m_cameraOffset = this.transform.InverseTransformPoint(m_camera.transform.position);

        StartCoroutine(UpdateCamera());
    }
}
