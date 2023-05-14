using UnityEngine;

[CreateAssetMenu()]
public class TLabWheelPhysics : ScriptableObject
{
    [Tooltip("ベースのグリップカーブ (μ/ms)")]
    public TLabLUT BaseGripCurve;
    [Tooltip("pecejka magic formulaに基づいたグリップカープ (μ/slipRatio)")]
    public TLabLUT SlipRatioGripCurve;
    [Tooltip("ARBの挙動を再現するグリップカーブ (μ/slipRatio)")]
    public TLabLUT RollGripCurve;
    [Tooltip("タイヤの滑り角に対する回転数の制御の強さ")]
    public TLabLUT AngleRatioCurve;
    [Tooltip("タイヤのトルクに対する回転数の制御の強さ")]
    public TLabLUT TorqueRatioCruve;

    public float spring = 75000;
    public float damper = 5000;
    public float wheelMass = 1f;
    public float susDst = 0.2f;
    public float targetPos = 0f;
    public float wheelRadius = 0.35f;
    public float arbFactor = 0f;

    // https://www.mitsui-direct.co.jp/car/guide/mycar_guide/new/stabilizer/
}
