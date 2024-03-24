using UnityEngine;

namespace TLab
{
    public class SceneManager : MonoBehaviour
    {
        public void LoadScene(string scene)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        }

        public void LoadScene(int index)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
    }
}
