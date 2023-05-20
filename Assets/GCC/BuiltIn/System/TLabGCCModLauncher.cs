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
}

[System.Serializable]
public class BuiltInMod
{
    public int index;
    public string url;
}