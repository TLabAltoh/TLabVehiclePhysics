using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateUVOnCollision
{
    // -----------------------------------------------------------------
    // ���̃v���O����, Mesh�����G��Plane�ł�����UV���v�Z�o���Ȃ�
    // ���ʃ��b�V���̕`��̎d���ɖ�肪����̂�?
    // ���ʂ��X���Ă݂��琳����uv���Z�o���邱�Ƃ��ł���
    // �� mvp�s��Ƃ̌v�Z��w������0�Ōv�Z���Ă������炾����

    // --------------------------------------------------------------------
    // ���̃v���O�����CGameObject�� Raycast�ŏՓ˔��肪�o����Ƃ��͕s�v
    // Collision�n�̏Փ˔���� Uv���W���v�Z����Ƃ��ɂ͕K�{

    #region �_p���|���S���̕ӂ̏�ɂ��邩����ςŔ���

    /// <summary>
    /// ���̊֐��Ŏ��ۂ�point���ӏ�ɂ��邩����ςŔ��肷��
    /// </summary>
    /// <param name="p">ObjectSpace�ɂ�����point</param>
    /// <param name="v1">ObjectSpace�ɂ�����|���S���̒��_</param>
    /// <param name="v2">ObjectSpace�ɂ�����|���S���̒��_</param>
    /// <returns>ExistPointOnTriangleEdge�ɐ^�U��Ԃ�</returns>
    public static bool ExistPointOnEdge(Vector3 p, Vector3 v1, Vector3 v2)
    {
        //TOLERANCE�͌덷�̋��e�͈�
        //�傫��1�̃x�N�g�����m�̓��ς��Ƃ��Ă���̂ŁA1���傫���Ȃ邱�Ƃ͂Ȃ�
        return 1 - 0.000001f < Vector3.Dot((v2 - p).normalized, (v2 - v1).normalized);
    }

    /// <summary>
    /// point���|���S���̕ӏ�ɂ��邩��F�X�ȕӂ̑g�ݍ��킹�Ŕ��肷��
    /// </summary>
    /// <param name="p">ObjectSpace�ɂ�����point</param>
    /// <param name="t1">ObjectSpace�ɂ�����|���S���̒��_</param>
    /// <param name="t2">����</param>
    /// <param name="t3">����</param>
    /// <returns>�ӂ̏�ɂ�������������������Ԃ�</returns>
    public static bool ExistPointOnTriangleEdge(Vector3 p, Vector3 t1, Vector3 t2, Vector3 t3)
    {
        if (ExistPointOnEdge(p, t1, t2) || ExistPointOnEdge(p, t2, t3) || ExistPointOnEdge(p, t3, t1))
        {
            //��̊֐��̎��s���ʂ�Ԃ�
            return true;
        }

        return false;
    }

    #endregion �_p���|���S���̕ӂ̏�ɂ��邩����ςŔ���

    #region ���X�N���v�g����Ăяo���֐�

    /// <summary>
    /// �󂯎������ԍ��W���|���S���̕\�ʂɈʒu���Ă��邩���v�Z����
    /// </summary>
    /// <param name="point">World��Ԃɂ�����Փˈʒu</param>
    /// <param name="meshFilter">point�\�ʂɂ��邩�𔻒肵����Object��meshFilter</param>
    /// <param name="transform">point�\�ʂɂ��邩�𔻒肵����Object��transfrom</param>
    /// <returns>�v�Z����uv���W��Ԃ�</returns>
    public Vector2 CalculateUV(Vector3 point, MeshFilter meshFilter, Transform transform)
    {
        var mesh = meshFilter.sharedMesh;
        Vector2 uv = new Vector2(2, 2);

        // mesh.triangles : mesh�̒��_�̕��т̔z��i{0, 1, 2, 0, ���}�݂����Ȃ�j
        for (var i = 0; i < mesh.triangles.Length; i += 3)
        {
            #region 1.����_p���^����ꂽ3�_�ɂ����ĕ��ʏ�ɑ��݂��邩

            var index0 = i + 0;
            var index1 = i + 1;
            var index2 = i + 2;

            // mesh.vertices : mesh��object��Ԃɂ�����ʒu
            var p1 = mesh.vertices[mesh.triangles[index0]];
            var p2 = mesh.vertices[mesh.triangles[index1]];
            var p3 = mesh.vertices[mesh.triangles[index2]];

            // object�ɑ΂���Փ˒n�_��local�ȍ��W
            // point��null�̂Ƃ�Vector3.zero��Ԃ��Ă���
            var p = transform.InverseTransformPoint(point);
            //Debug.Log(p + "," + point);

            var v1 = p2 - p1;
            var v2 = p3 - p1;
            var vp = p - p1;

            // �C�ӂ̓_p�Ɗe�|���S���̊O�ς����߂�, ���������������Ă��邩�𒲂ׂ�
            var nv = Vector3.Cross(v1, v2);
            var val = Vector3.Dot(nv, vp);

            var suc = -0.000001f < val && val < 0.000001f;

            #endregion 1.����_p���^����ꂽ3�_�ɂ����ĕ��ʏ�ɑ��݂��邩

            #region 2.���ꕽ�ʏ�ɑ��݂���_p���O�p�`�����ɑ��݂��邩

            if (!suc)
                continue;
            else
            {
                var a = Vector3.Cross(p1 - p3, p - p1).normalized;
                var b = Vector3.Cross(p2 - p1, p - p2).normalized;
                var c = Vector3.Cross(p3 - p2, p - p3).normalized;

                var d_ab = Vector3.Dot(a, b);
                var d_bc = Vector3.Dot(b, c);

                suc = 0.999f < d_ab && 0.999f < d_bc || ExistPointOnTriangleEdge(p, p1, p2, p3);
            }

            #endregion 2.���ꕽ�ʏ�ɑ��݂���_p���O�p�`�����ɑ��݂��邩

            #region 3.�_p��UV���W�����߂�

            if (!suc)
                continue;
            else
            {
                // mesh.vertices�ɑΉ�����uv���W
                var uv1 = mesh.uv[mesh.triangles[index0]];
                var uv2 = mesh.uv[mesh.triangles[index1]];
                var uv3 = mesh.uv[mesh.triangles[index2]];

                //PerspectiveCollect(�������e���l������UV���)
                Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * transform.localToWorldMatrix;

                // �e�_��ProjectionSpace�ւ̕ϊ�(���ۂɂ͍��Ɋ|���Ă���((�R���s���[�^�Ȃ̂ŉ��x�N�g��)))
                // w��viewSpace�ɂ�����z���W
                // �F�X���������̂�url�\���Ă���
                // �x�N�g����w������0�Ōv�Z���Ă����̂ŁA���s�ړ��̍s�񂪍�p���Ă��Ȃ������B
                // ������͂܂�local to world�ϊ����o���Ă��Ȃ�����
                /* https://esprog.hatenablog.com/entry/2016/10/09/062952 */

                Vector4 p1_p = mvp * new Vector4(p1.x, p1.y, p1.z, 1);
                Vector4 p2_p = mvp * new Vector4(p2.x, p2.y, p2.z, 1);
                Vector4 p3_p = mvp * new Vector4(p3.x, p3.y, p3.z, 1);
                Vector4 p_p = mvp * new Vector4(p.x, p.y, p.z, 1);

                Vector2 p1_n = new Vector2(p1_p.x, p1_p.y) / p1_p.w;
                Vector2 p2_n = new Vector2(p2_p.x, p2_p.y) / p2_p.w;
                Vector2 p3_n = new Vector2(p3_p.x, p3_p.y) / p3_p.w;
                Vector2 p_n = new Vector2(p_p.x, p_p.y) / p_p.w;
                // ���_�̂Ȃ��O�p�`��_p�ɂ��3�������A�K�v�ɂȂ�ʐς��v�Z
                var s = 0.5f * ((p2_n.x - p1_n.x) * (p3_n.y - p1_n.y) - (p2_n.y - p1_n.y) * (p3_n.x - p1_n.x));
                var s1 = 0.5f * ((p3_n.x - p_n.x) * (p1_n.y - p_n.y) - (p3_n.y - p_n.y) * (p1_n.x - p_n.x));
                var s2 = 0.5f * ((p1_n.x - p_n.x) * (p2_n.y - p_n.y) - (p1_n.y - p_n.y) * (p2_n.x - p_n.x));
                // �ʐϔ䂩��uv����
                var u = s1 / s;
                var v = s2 / s;
                var w = 1 / ((1 - u - v) * 1 / p1_p.w + u * 1 / p2_p.w + v * 1 / p3_p.w);
                uv = w * ((1 - u - v) * uv1 / p1_p.w + u * uv2 / p2_p.w + v * uv3 / p3_p.w);
                return uv;
            }
            #endregion 3.�_p��UV���W�����߂�
        }
        return uv;
    }

    #endregion ���X�N���v�g����Ăяo���֐�
}
