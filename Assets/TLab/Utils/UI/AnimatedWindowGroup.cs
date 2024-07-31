using System.Collections;
using UnityEngine;

namespace TLab.UI
{
    [System.Serializable]
    public class AnimatedWindow
    {
        [SerializeField] private Animator m_animator;
        [SerializeField] private GameObject m_window;

        [Header("Animation State")]
        [SerializeField] private string m_hideState;
        [SerializeField] private string m_showState;

        [Header("Animation")]
        [SerializeField] private string m_hideAnim;
        [SerializeField] private string m_showAnim;

        [Header("Deley")]
        [SerializeField] private float m_showDeley = 0.25f;
        [SerializeField] private float m_hideDeley = 0.25f;

        public bool active => m_window.activeSelf;

        private MonoBehaviour m_mono;

        private bool SetCurrent(in MonoBehaviour mono)
        {
            m_mono = mono;

            return (m_animator != null) && (m_mono != null);
        }

        private IEnumerator SetActiveTask(bool active)
        {
            if (active)
            {
                m_window.SetActive(true);

                m_animator.Play(m_showAnim);

                yield return new WaitForSeconds(m_showDeley);

                m_animator.Play(m_showState);
            }
            else
            {
                m_animator.Play(m_hideAnim);

                yield return new WaitForSeconds(m_hideDeley);

                m_animator.Play(m_hideState);

                m_window.SetActive(false);
            }
        }

        public void SetActive(in MonoBehaviour mono, bool active)
        {
            if (!SetCurrent(in mono))
            {
                return;
            }

            if (this.active == active)
            {
                return;
            }

            mono.StartCoroutine(SetActiveTask(active));
        }

        public void SwitchActive(in MonoBehaviour mono)
        {
            if (!SetCurrent(in mono))
            {
                return;
            }

            bool active = !this.active;

            mono.StartCoroutine(SetActiveTask(active));
        }
    }

    public class AnimatedWindowGroup : MonoBehaviour
    {
        [SerializeField] private AnimatedWindow[] m_window;

        private bool CheckRange(int index)
        {
            return index <= m_window.Length;
        }

        public void SwitchWindowActive(int index)
        {
            if (CheckRange(index))
            {
                m_window[index].SwitchActive(this);
            }
        }

        public void SetWindowActive(int index, bool active)
        {
            if (CheckRange(index))
            {
                m_window[index].SetActive(this, active);
            }
        }

        public void SetWindowOnly(int index)
        {
            if (CheckRange(index))
            {
                for (int i = 0; i < m_window.Length; i++)
                {
                    m_window[i].SetActive(this, (i == index));
                }
            }
        }
    }
}
