using UnityEngine;
using UnityEngine.Events;

namespace TLab
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private LayerMask m_target;

        [SerializeField] private bool m_active = false;

        [SerializeField] private UnityEvent m_onPass;

        private bool CompareLayer(int layer)
        {
            return ((1 << layer) & m_target) != 0;
        }

        public void SetActive(bool active)
        {
            m_active = active;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_active && CompareLayer(other.gameObject.layer))
            {
                m_onPass.Invoke();
            }
        }
    }
}
