using UnityEngine;

public class TLabGCCModLauncher : MonoBehaviour
{
    [Header("Custom Mod URL")]
    [SerializeField] TLabInputField m_modURL;

    [Header("Wheel Physics")]
    [SerializeField] TLabWheelPhysics m_rearPhysics;
    [SerializeField] TLabWheelPhysics m_frontPhysics;

    [Header("Buildin Mod URL")]
    [SerializeField] BuiltInMod[] m_builtinMods;

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