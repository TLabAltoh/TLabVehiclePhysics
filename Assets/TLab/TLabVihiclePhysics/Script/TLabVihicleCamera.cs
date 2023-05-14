using System.Collections;
using UnityEngine;

public class TLabVihicleCamera : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        TherdPerson
    };

    [Tooltip("What person camera to use")]
    [SerializeField] CameraMode m_cameraMode;

    [Header("FPS Settings")]
    [SerializeField] int m_maxVertical = 60;
    [SerializeField] int m_maxHorizontal = 60;

    [Header("TPS Settings")]
    [SerializeField] float m_cameraDistance = 3.2f;
    [SerializeField] float m_cameraHeight = 0.8f;

    private Transform lookObj;
    private Rigidbody m_rb;
    private Vector3 lookDir;
    private Vector3 forwardLook;
    private Vector3 upLook;
    private Vector3 fpsPos;
    private float xInput;
    private float yInput;
    private float actualVertical;
    private float actualHorizontal;
    private float smoothYRot;

    public void SetInputFromKeyBorad(float x, float y)
    {
        xInput = x;
        yInput = y;
    }

    private void FPSCamera()
    {
        //
        // Update first person camera
        //

        // Update camera position
        Camera.main.transform.position = transform.TransformPoint(fpsPos);

        // Update camera rotation
        actualVertical = Mathf.Lerp(actualVertical, m_maxVertical * yInput, 0.1f);
        actualHorizontal = Mathf.Lerp(actualHorizontal, m_maxHorizontal * xInput, 0.1f);
        Quaternion quaternion = transform.rotation;
        Camera.main.transform.rotation = Quaternion.Euler(quaternion.eulerAngles.x + actualVertical, quaternion.eulerAngles.y + actualHorizontal,quaternion.eulerAngles.z);
    }

    public void TPSCamera()
    {
        //
        // Update third person camera
        //
        
        lookDir = Vector3.Slerp(lookDir, (xInput == 0 && yInput == 0 ? Vector3.forward : new Vector3(xInput, 0, yInput).normalized), 0.1f);
        smoothYRot = Mathf.Lerp(smoothYRot, m_rb.angularVelocity.y, 0.02f);
        
        forwardLook = Vector3.Lerp(forwardLook, transform.forward, 0.05f);
        upLook = Vector3.Lerp(upLook, transform.up, 0.05f);
        
        lookObj.rotation = Quaternion.LookRotation(forwardLook, upLook);
        lookObj.position = transform.position;
        
        Vector3 lookDirActual = (lookDir - new Vector3(Mathf.Sin(smoothYRot), 0, Mathf.Cos(smoothYRot)) * Mathf.Abs(smoothYRot) * 0.2f).normalized;
        Vector3 forwardDir = lookObj.TransformDirection(lookDirActual);
        Vector3 localOffset = lookObj.TransformPoint(-lookDirActual * m_cameraDistance - lookDirActual * Mathf.Min(m_rb.velocity.magnitude * 0.05f, 2) + Vector3.up * m_cameraHeight);

        Camera.main.transform.position = localOffset;
        Camera.main.transform.rotation = Quaternion.LookRotation(forwardDir, lookObj.up);
    }

    public void SwitchCameraMode()
    {
        if (m_cameraMode == CameraMode.FirstPerson)
        {
            m_cameraMode = CameraMode.TherdPerson;
            forwardLook = transform.forward;
        }
        else if (m_cameraMode == CameraMode.TherdPerson)
        {
            m_cameraMode = CameraMode.FirstPerson;
        }
    }

    IEnumerator UpdateCameraPosition()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if(Camera.main != null)
            {
                if (m_cameraMode == CameraMode.FirstPerson)
                {
                    FPSCamera();
                }
                else if (m_cameraMode == CameraMode.TherdPerson)
                {
                    TPSCamera();
                }
            }
        }
    }

    private void Awake()
    {
        Camera.main.transform.parent = null;
    }

    void Start()
    {
        GameObject lookTemp = new GameObject("Camera Looker");
        lookObj = lookTemp.transform;

        m_rb = GetComponent<Rigidbody>();

        fpsPos = this.transform.InverseTransformPoint(Camera.main.transform.position);

        StartCoroutine(UpdateCameraPosition());
    }
}
