using UnityEngine;
using UnityEngine.UIElements;
using TLab.VehiclePhysics;

public class VehicleDebugger : MonoBehaviour
{
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

    void Update()
    {
        VisualElement debugField;
        Label value;
        VisualElement rootVE = m_uiDocument.rootVisualElement;

        debugField = rootVE.Q<VisualElement>("rpm");
        value = debugField.Q<Label>("value");
        value.text = m_engine.engineRpm.ToString();

        debugField = rootVE.Q<VisualElement>("gear");
        value = debugField.Q<Label>("value");
        value.text = m_engine.currentGear.ToString();

        debugField = rootVE.Q<VisualElement>("speed");
        value = debugField.Q<Label>("value");
        value.text = ((int)m_physics.KilometerPerHourInLocal).ToString();
    }
}
