using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TLab.VehiclePhysics.Input
{
    public class VirtualInputAxis : MonoBehaviour
    {
        public enum InputMethod
        {
            Keyborad,
            UIButton
        }

        [SerializeField] InputMethod m_inputMethod;

        [Header("Keyborad")]
        [SerializeField] private KeyCode m_positiveKey;
        [SerializeField] private KeyCode m_negativeKey;

        [Header("UI Button")]
        [SerializeField] private float moveSpeed;
        [SerializeField] private float getBackSpeed;
        [SerializeField] private Button positiveButton;
        [SerializeField] private Button negativeButton;

        private bool m_positivePressed = false;
        private bool m_negativePressed = false;
        private EventTrigger m_positiveButtonEvent;
        private EventTrigger m_negativeButtonEvent;

        private float m_axisValue = 0;

        public float AxisValue => m_axisValue;

        public void PositiveButtonDown(PointerEventData data) => m_positivePressed = true;

        public void PositiveButtonUp(PointerEventData data) => m_positivePressed = false;

        public void NegativeButtonDown(PointerEventData data) => m_negativePressed = true;

        public void NegativeButtonUp(PointerEventData data) => m_negativePressed = false;

        void Start()
        {
            if (positiveButton != null)
            {
                m_positiveButtonEvent = positiveButton.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entryDown = new EventTrigger.Entry();
                entryDown.eventID = EventTriggerType.PointerDown;
                entryDown.callback.AddListener((data) => { PositiveButtonDown((PointerEventData)data); });
                m_positiveButtonEvent.triggers.Add(entryDown);

                EventTrigger.Entry entryUp = new EventTrigger.Entry();
                entryUp.eventID = EventTriggerType.PointerUp;
                entryUp.callback.AddListener((data) => { PositiveButtonUp((PointerEventData)data); });
                m_positiveButtonEvent.triggers.Add(entryUp);
            }

            if (negativeButton != null)
            {
                m_negativeButtonEvent = negativeButton.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entryDown = new EventTrigger.Entry();
                entryDown.eventID = EventTriggerType.PointerDown;
                entryDown.callback.AddListener((data) => { NegativeButtonDown((PointerEventData)data); });
                m_negativeButtonEvent.triggers.Add(entryDown);

                EventTrigger.Entry entryUp = new EventTrigger.Entry();
                entryUp.eventID = EventTriggerType.PointerUp;
                entryUp.callback.AddListener((data) => { NegativeButtonUp((PointerEventData)data); });
                m_negativeButtonEvent.triggers.Add(entryUp);
            }
        }

        void Update()
        {
            switch (m_inputMethod)
            {
                case InputMethod.Keyborad:
                    m_positivePressed = UnityEngine.Input.GetKey(m_positiveKey);
                    m_negativePressed = UnityEngine.Input.GetKey(m_negativeKey);
                    break;
                case InputMethod.UIButton:
                    break;
            }

            bool noInput = false;

            int dir = 0;
            if (m_positivePressed && m_negativePressed)
                dir = 0;
            else if (m_positivePressed)
                dir = 1;
            else if (m_negativePressed)
                dir = -1;
            else
                noInput = true;

            if (noInput)
            {
                if (Mathf.Abs(m_axisValue) >= 0.01f)
                {
                    if (m_axisValue > 0)
                    {
                        m_axisValue -= Time.deltaTime * getBackSpeed;
                        m_axisValue = (m_axisValue >= 0) ? m_axisValue : 0;
                    }
                    else
                    {
                        m_axisValue += Time.deltaTime * getBackSpeed;
                        m_axisValue = (m_axisValue <= 0) ? m_axisValue : 0;
                    }
                }
                else
                    m_axisValue = 0;
            }
            else
            {
                m_axisValue += Time.deltaTime * dir * moveSpeed;
                m_axisValue = Mathf.Clamp(m_axisValue, -1, 1);
            }
        }
    }
}
