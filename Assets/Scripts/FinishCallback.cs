using UnityEngine;
using TMPro;

namespace TLab
{
    public class FinishCallback : MonoBehaviour
    {
        [SerializeField] private Timer m_timer;

        [SerializeField] private GameObject m_wndFinish;

        [SerializeField] private TextMeshProUGUI m_txtFinishTime;

        public void OnFinish()
        {
            m_timer.StopTimer();

            m_wndFinish.SetActive(true);

            m_txtFinishTime.text = $"Time: {m_timer.elapsed.ToString("0.000")}";

            Debug.Log(m_timer.elapsed);

            Destroy(m_timer.gameObject);
        }
    }
}
