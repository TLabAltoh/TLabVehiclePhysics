using UnityEngine;
using UnityEngine.Events;

namespace TLab.Game.TimeAttack
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private LayerMask m_target;

        [SerializeField] private int m_index;

        [SerializeField] private bool m_active = false;

        [SerializeField] private UnityEvent<int> m_onPass;

        private bool CompareLayer(int layer)
        {
            return ((1 << layer) & m_target) != 0;
        }

        public void SetActive(bool active)
        {
            m_active = active;
        }

        public void SetUp(int i, LayerMask layerMask)
        {
            m_index = i;

            m_target = layerMask;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_active && CompareLayer(other.gameObject.layer))
            {
                m_onPass.Invoke(m_index);
            }
        }
    }
}
