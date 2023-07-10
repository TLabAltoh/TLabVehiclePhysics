using UnityEngine;
using TLab;

[CreateAssetMenu()]
public class TLabWheelPhysics : ScriptableObject
{
    [Tooltip("�x�[�X�̃O���b�v�J�[�u (��/ms)")]
    public TLabLUT BaseGripCurve;
    [Tooltip("pecejka magic formula�Ɋ�Â����O���b�v�J�[�v (��/slipRatio)")]
    public TLabLUT SlipRatioGripCurve;
    [Tooltip("�^�C���̊���p�ɑ΂����]���̐���̋���")]
    public TLabLUT AngleRatioCurve;
    [Tooltip("�^�C���̃g���N�ɑ΂����]���̐���̋���")]
    public TLabLUT TorqueRatioCruve;

    public float spring = 75000;
    public float damper = 5000;
    public float wheelMass = 1f;
    public float susDst = 0.2f;
    public float targetPos = 0f;
    public float wheelRadius = 0.35f;
}
