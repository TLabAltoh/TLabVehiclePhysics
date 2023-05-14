using UnityEngine;

[CreateAssetMenu()]
public class TLabVihicleEngineInfo : ScriptableObject
{
    [Header("Gear info")]
    public Gear[] gears;

    [Header("Engine shaft torque")]
    public TLabLUT rpmTorqueCurve;
}

[System.Serializable]
public class Gear
{
    public int gear;
    public float ratio;
}

