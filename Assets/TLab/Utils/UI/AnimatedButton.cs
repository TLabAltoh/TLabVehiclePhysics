using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TLab.UI
{
    public enum PointerEvent
    {
        ON_POINTER_ENTER,
        ON_POINTER_EXIT,
        ON_POINTER_CLICK
    }

    [System.Serializable]
    public class ButtonAnimatorEvent
    {
        [SerializeField] private float m_deley = 0.25f;

        [SerializeField] private UnityEvent m_onFired;

        private Animator m_animator;
        private MonoBehaviour m_mono;

        private bool SetCurrent(in Animator animator, in MonoBehaviour mono)
        {
            m_animator = animator;

            m_mono = mono;

            return (m_animator != null) && (m_mono != null);
        }

        private IEnumerator Deley(PointerEvent pointerEvent)
        {
            switch (pointerEvent)
            {
                case PointerEvent.ON_POINTER_CLICK:
                    m_animator.Play("button_on_pointer_click");
                    break;
                case PointerEvent.ON_POINTER_ENTER:
                    m_animator.Play("button_on_pointer_enter");
                    break;
                case PointerEvent.ON_POINTER_EXIT:
                    m_animator.Play("button_on_pointer_exit");
                    break;
            }

            yield return new WaitForSeconds(m_deley);

            m_onFired.Invoke();
        }

        public void Play(in Animator animator, in MonoBehaviour mono, PointerEvent pointerEvent)
        {
            if (SetCurrent(in animator, in mono))
            {
                mono.StartCoroutine(Deley(pointerEvent));
            }
        }
    }

    public class AnimatedButton : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Animator m_animator;

        [SerializeField] private ButtonAnimatorEvent m_onPointerEnter;
        [SerializeField] private ButtonAnimatorEvent m_onPointerExit;
        [SerializeField] private ButtonAnimatorEvent m_onPoitnerClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            m_onPoitnerClick.Play(in m_animator, this, PointerEvent.ON_POINTER_CLICK);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_onPointerEnter.Play(in m_animator, this, PointerEvent.ON_POINTER_ENTER);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_onPointerExit.Play(in m_animator, this, PointerEvent.ON_POINTER_EXIT);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            m_animator = GetComponent<Animator>();

            if (m_animator == null)
            {
                m_animator = gameObject.AddComponent<Animator>();
            }
        }
#endif
    }
}
