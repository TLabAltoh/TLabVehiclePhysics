using UnityEngine;

namespace TLab
{
    public class DescriptionFade : MonoBehaviour
    {
        [SerializeField] private Animator m_animator;

        public void OnPointerEnter()
        {
            m_animator.Play("description_fadein", 0);
        }

        public void OnPointerExit()
        {
            m_animator.Play("description_fadeout", 0);
        }
    }
}
