using UnityEngine;
using UnityEngine.SceneManagement;

public class TLabGCCMapManager : MonoBehaviour
{
    [SerializeField] Vector3 respown;

    public static TLabGCCMapManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameObject obj = (GameObject)Resources.Load("TLabCarRoot");
        Instantiate(obj, respown, Quaternion.identity);
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }
}
