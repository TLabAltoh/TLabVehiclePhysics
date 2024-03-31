using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TLab
{
    public class CountDown : MonoBehaviour
    {
        [SerializeField] private Animator[] m_animator;

        [SerializeField] private float m_deley = 1.0f;

        [SerializeField] private UnityEvent m_befor;
        [SerializeField] private UnityEvent m_after;

        private IEnumerator CountDownTask()
        {
            m_befor.Invoke();

            yield return new WaitForSeconds(m_deley);

            var animator = null as Animator;

            for (int i = 0; i < m_animator.Length - 1; i++)
            {
                animator = m_animator[i];

                animator.Play("count_down_number", 0, 0);

                do
                {
                    yield return null;
                }
                while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

                animator.gameObject.SetActive(false);
            }

            animator = m_animator[m_animator.Length - 1];

            animator.Play("count_down_start", 0, 0);

            m_after.Invoke();

            do
            {
                yield return null;
            }
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

            animator.gameObject.SetActive(false);

            GameObject.Destroy(this.gameObject);
        }

        public void StartCountDown()
        {
            StartCoroutine(CountDownTask());
        }

        private void Start()
        {
            StartCountDown();
        }
    }
}
