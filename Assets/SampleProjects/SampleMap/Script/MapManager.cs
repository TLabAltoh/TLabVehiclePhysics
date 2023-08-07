using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
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
