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

    private void Awake()
    {
        Instance = this;
    }
}
