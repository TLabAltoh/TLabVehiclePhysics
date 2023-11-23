using UnityEngine;
using UnityEngine.UIElements;
using TLab.VehiclePhysics;

public class VehicleDebugger : MonoBehaviour
{
    [SerializeField] private WheelColliderSource m_rearWheel;
    [SerializeField] private WheelColliderSource m_frontWheel;

    [SerializeField] private VehiclePhysics m_physics;
    [SerializeField] private VehicleEngine m_engine;

    [SerializeField] private UIDocument m_uiDocument;

    void Start()
    {
        VisualElement rootVE = m_uiDocument.rootVisualElement;
        Button button = rootVE.Q<Button>("reset");
        button.clicked += () =>
        {
            this.transform.parent.up = Vector3.up;
        };
    }

    private void DebugInfo(VisualElement rootVE, string labelName, string v)
    {
        VisualElement debugField;
        Label value;

        debugField = rootVE.Q<VisualElement>(labelName);
        value = debugField.Q<Label>("value");
        value.text = v;
    }

    void Update()
    {
        VisualElement rootVE = m_uiDocument.rootVisualElement;
        string value = "";

        value = m_engine.engineRpm.ToString();
        DebugInfo(rootVE, "rpm", value);

        value = m_engine.currentGear.ToString();
        DebugInfo(rootVE, "gear", value);

        value = ((int)m_physics.minAngle).ToString();
        DebugInfo(rootVE, "angle", value);

        value = ((int)m_physics.kilometerPerHourInLocal).ToString();
        DebugInfo(rootVE, "speed", value);

        value = m_rearWheel.gripFactor.ToString();
        DebugInfo(rootVE, "grip_r", value);

        value = m_frontWheel.gripFactor.ToString();
        DebugInfo(rootVE, "grip_l", value);
    }
}
