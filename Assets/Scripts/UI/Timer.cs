using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_time;

    private float m_elapsed = 0.0f;

    private bool m_running = false;

    public void SetActive(bool active)
    {
        m_time.gameObject.SetActive(active);
    }

    public void StartTimer()
    {
        m_running = true;
    }

    public void StopTimer()
    {
        m_running = false;
    }

    public void ResetTimer()
    {
        m_elapsed = 0.0f;

        m_time.text = 0.0f.ToString("0.00");
    }

    public void Update()
    {
        if (m_running)
        {
            m_elapsed += Time.deltaTime;
        }

        m_time.text = m_elapsed.ToString("0.00");
    }
}
