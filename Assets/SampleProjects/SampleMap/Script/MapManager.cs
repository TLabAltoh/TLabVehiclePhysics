using UnityEngine;
using UnityEngine.SceneManagement;
using TLab.VihiclePhysics;

public class MapManager : MonoBehaviour
{
    [SerializeField] WheelPhysics m_wheelRear;
    [SerializeField] WheelPhysics m_wheelFront;

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
