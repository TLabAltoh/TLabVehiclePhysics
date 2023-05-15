using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MapManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_arbRearText;
    [SerializeField] TextMeshProUGUI m_arbFrontText;
    [SerializeField] Slider m_arbRear;
    [SerializeField] Slider m_arbFront;
    [SerializeField] TLabWheelPhysics m_wheelRear;
    [SerializeField] TLabWheelPhysics m_wheelFront;

    public static MapManager Instance;

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

    public void SetRearARB()
    {
        float value = (int)(m_arbRear.value * 100f) / 100f;
        m_arbRearText.text = value.ToString();
        m_wheelRear.arbFactor = value;
    }

    public void SetFrontARB()
    {
        float value = (int)(m_arbFront.value * 100f) / 100f;
        m_arbFrontText.text = value.ToString();
        m_wheelFront.arbFactor = value;
    }

    private void Awake()
    {
        Instance = this;
    }
}
