using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TLab
{
    public class CustomSceneManager : MonoBehaviour
    {
        private static string sceneName = null;

        [SerializeField] private Fade m_fade;
        [SerializeField] private FadeSettings m_in;
        [SerializeField] private FadeSettings m_out;

        private IEnumerator LoadSceneTask(string sceneName)
        {
            m_fade.Call(m_out);

            yield return new WaitForSeconds(m_out.delay);

            CustomSceneManager.sceneName = sceneName;

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneTask(sceneName));
        }

        private void Start()
        {
            m_fade.Call(m_in);
        }
    }
}
