using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VirtualInput : MonoBehaviour
{
    [Header("Input config")]
    [SerializeField] float moveSpeed;
    [SerializeField] float getBackSpeed;
    [SerializeField] Button positiveButton;
    [SerializeField] Button negativeButton;

    private float inputValue = 0;
    private bool positivePressed = false;
    private bool negativePressed = false;
    private EventTrigger positiveButtonEvent;
    private EventTrigger negativeButtonEvent;

    public float InputValue
    {
        get
        {
            return inputValue;
        }
    }

    public void PositiveButtonDown(PointerEventData data)
    {
        positivePressed = true;
    }

    public void PositiveButtonUp(PointerEventData data)
    {
        positivePressed = false;
    }

    public void NegativeButtonDown(PointerEventData data)
    {
        negativePressed = true;
    }

    public void NegativeButtonUp(PointerEventData data)
    {
        negativePressed = false;
    }

    void Start()
    {
        if(positiveButton != null)
        {
            positiveButtonEvent = positiveButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { PositiveButtonDown((PointerEventData)data); });
            positiveButtonEvent.triggers.Add(entryDown);

            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { PositiveButtonUp((PointerEventData)data); });
            positiveButtonEvent.triggers.Add(entryUp);
        }

        if (negativeButton != null)
        {
            negativeButtonEvent = negativeButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { NegativeButtonDown((PointerEventData)data); });
            negativeButtonEvent.triggers.Add(entryDown);

            EventTrigger.Entry entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { NegativeButtonUp((PointerEventData)data); });
            negativeButtonEvent.triggers.Add(entryUp);
        }
    }

    void Update()
    {
        bool noInput = false;

        int dir = 0;
        if (positivePressed && negativePressed)
            dir = 0;
        else if (positivePressed)
            dir = 1;
        else if (negativePressed)
            dir = -1;
        else
            noInput = true;

        if (noInput)
        {
            if (Mathf.Abs(inputValue) >= 0.01f)
            {
                if (inputValue > 0)
                {
                    inputValue -= Time.deltaTime * getBackSpeed;
                    inputValue = (inputValue >= 0) ? inputValue : 0;
                }
                else
                {
                    inputValue += Time.deltaTime * getBackSpeed;
                    inputValue = (inputValue <= 0) ? inputValue : 0;
                }
            }
            else
                inputValue = 0;
        }
        else
        {
            inputValue += Time.deltaTime * dir * moveSpeed;
            inputValue = Mathf.Clamp(inputValue, -1, 1);
        }
    }
}
