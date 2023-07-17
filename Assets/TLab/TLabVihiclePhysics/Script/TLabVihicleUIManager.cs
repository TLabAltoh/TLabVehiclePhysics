using UnityEngine;

public class TLabVihicleUIManager : MonoBehaviour
{
    [SerializeField] Transform m_car;
    [SerializeField] GameObject m_backMirror;

    [Header("Audio")]
    [SerializeField] AudioSource clickAudio;

    public void OnSwitchCamera()
    {
        m_backMirror.SetActive(!(m_backMirror.activeSelf));
    }

    public void OnResetRotateButtoPress()
    {
        clickAudio.Play();
        m_car.transform.up = Vector3.up;
    }
}
