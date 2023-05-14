using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TLabGCCModLauncher : MonoBehaviour
{
    [SerializeField] TLabInputField m_modURL;
    [SerializeField] Slider m_arbRearSlider;
    [SerializeField] Slider m_arbFrontSlider;
    [SerializeField] TextMeshProUGUI m_arbRear;
    [SerializeField] TextMeshProUGUI m_arbFront;
    [SerializeField] TLabWheelPhysics m_rearPhysics;
    [SerializeField] TLabWheelPhysics m_frontPhysics;

    [SerializeField] BuiltInMod[] m_builtinMods;

    // "modtestmap.assetbundl"

    public void StartMod()
    {
        if(m_modURL.text != null)
            TLabGCCSystemManager.Instance.LoadMod(m_modURL.text);
    }

    public void StartBuiltInMod(int index)
    {
        TLabGCCSystemManager.Instance.LoadMod(m_builtinMods[index].url);
    }

    public void SetARBRear()
    {
        float value = (int)(m_arbRearSlider.value * 100) / 100f;
        m_rearPhysics.arbFactor = value;
        m_arbRear.text = value.ToString();
    }

    public void SetARBFront()
    {
        float value = (int)(m_arbFrontSlider.value * 100) / 100f;
        m_frontPhysics.arbFactor = value;
        m_arbFront.text = value.ToString();
    }
}

[System.Serializable]
public class BuiltInMod
{
    public int index;
    public string url;
}