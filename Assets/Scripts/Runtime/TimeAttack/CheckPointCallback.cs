using UnityEngine;
using UnityEngine.Events;

namespace TLab.Game.TimeAttack
{
    public class CheckPointCallback : MonoBehaviour
    {
        [SerializeField] private int m_goal;
        [SerializeField] private Material m_matGoal;
        [SerializeField] private Material m_matSleep;
        [SerializeField] private Material m_matTarget;
        [SerializeField] private LayerMask m_target;
        [SerializeField] private UnityEvent m_onFinish;

        private int m_goalGatePassCount = 0;

        public int goal => m_goal;

        public Material matGoal => m_matGoal;

        public Material matSleep => m_matSleep;

        public Material matTarget => m_matTarget;

        public LayerMask target => m_target;

        public void OnCheckPointPass(int index)
        {
            var current = transform.GetChild(index);
            current.GetComponent<CheckPoint>().SetActive(false);

            if (index == m_goal)
            {
                m_goalGatePassCount++;

                if (m_goalGatePassCount > 1)
                {
                    m_onFinish.Invoke();

                    return;
                }
            }

            current.GetComponent<MeshRenderer>().material = m_matSleep;

            var nextIndex = (index + 1) % transform.childCount;

            var next = transform.GetChild(nextIndex);
            next.GetComponent<CheckPoint>().SetActive(true);

            next.GetComponent<MeshRenderer>().material =
                (nextIndex != m_goal) ? m_matTarget : m_matGoal;
        }
    }
}
