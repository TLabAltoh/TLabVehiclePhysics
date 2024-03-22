using UnityEngine;
using UnityEngine.UIElements;
using TLab.VehiclePhysics;

public class VehicleDebugger : MonoBehaviour
{
    [SerializeField] private WheelColliderSource m_wheelFR;
    [SerializeField] private WheelColliderSource m_wheelFL;
    [SerializeField] private WheelColliderSource m_wheelRR;
    [SerializeField] private WheelColliderSource m_wheelRL;

    [SerializeField] private VehiclePhysics m_physics;
    [SerializeField] private VehicleEngine m_engine;

    [SerializeField] private UIDocument m_uiDocument;

    private Label m_labelRpm;
    private Label m_labelGear;
    private Label m_labelAngle;
    private Label m_labelSpeed;
    private Label m_labelGripR;
    private Label m_labelGripF;

    private VisualElement m_gageSlipBG;
    private VisualElement m_gageSlipFR;
    private VisualElement m_gageSlipFL;
    private VisualElement m_gageSlipRR;
    private VisualElement m_gageSlipRL;

    void Start()
    {
        VisualElement rootVE = m_uiDocument.rootVisualElement;
        Button button = rootVE.Q<Button>("reset");
        button.clicked += () =>
        {
            this.transform.parent.up = Vector3.up;
        };

        m_labelRpm = rootVE.GetElement<Label>("rpm", "value");
        m_labelGear = rootVE.GetElement<Label>("gear", "value");
        m_labelAngle = rootVE.GetElement<Label>("angle", "value");
        m_labelSpeed = rootVE.GetElement<Label>("speed", "value");
        m_labelGripR = rootVE.GetElement<Label>("grip_r", "value");
        m_labelGripF = rootVE.GetElement<Label>("grip_f", "value");

        m_gageSlipBG = rootVE.GetElement<VisualElement>("slip", "front", "left", "bg");
        m_gageSlipFL = rootVE.GetElement<VisualElement>("slip", "front", "left", "bg", "fill");
        m_gageSlipFR = rootVE.GetElement<VisualElement>("slip", "front", "right", "bg", "fill");
        m_gageSlipRL = rootVE.GetElement<VisualElement>("slip", "rear", "left", "bg", "fill");
        m_gageSlipRR = rootVE.GetElement<VisualElement>("slip", "rear", "right", "bg", "fill");
    }

    void Update()
    {
        m_labelRpm.text = m_engine.engineRpm.ToString();
        m_labelGear.text = m_engine.gear.ToString();
        m_labelAngle.text = m_physics.minAngle.ToString("0.00");
        m_labelSpeed.text = m_physics.kilometerPerHourInLocal.ToString("0.0");
        m_labelGripR.text = m_physics.downforceRear.ToString("0.00");
        m_labelGripF.text = m_physics.downforceFront.ToString("0.00");

        float height = m_gageSlipBG.resolvedStyle.height;
        m_gageSlipFL.style.height = Mathf.Abs(m_wheelFL.feedbackSlipRatio) * height;
        m_gageSlipFR.style.height = Mathf.Abs(m_wheelFR.feedbackSlipRatio) * height;
        m_gageSlipRL.style.height = Mathf.Abs(m_wheelRL.feedbackSlipRatio) * height;
        m_gageSlipRR.style.height = Mathf.Abs(m_wheelRR.feedbackSlipRatio) * height;
    }
}

public static class VisualElementExtension
{
    public static T GetElement<T>(this VisualElement rootVE, params string[] elementNameLevel) where T : VisualElement
    {
        VisualElement element = rootVE;

        for (int i = 0; i < elementNameLevel.Length - 1; i++)
        {
            element = element.Q<VisualElement>(elementNameLevel[i]);
        }

        return element.Q<T>(elementNameLevel[elementNameLevel.Length - 1]); ;
    }
}
