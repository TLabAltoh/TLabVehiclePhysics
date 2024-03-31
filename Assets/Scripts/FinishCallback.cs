using UnityEngine;

namespace TLab
{
    public class FinishCallback : MonoBehaviour
    {
        [SerializeField] private Timer m_timer;

        public void OnFinish()
        {
            m_timer.StopTimer();

            Debug.Log(m_timer.elapsed);

            Destroy(m_timer.gameObject);
        }
    }
}
