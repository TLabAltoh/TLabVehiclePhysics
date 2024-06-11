using System.Collections.Generic;
using UnityEngine;

namespace TLab.VehiclePhysics
{
    public class DrawWheel : MonoBehaviour
    {
        [SerializeField] private WheelColliderSource[] m_wheelColliderSources;

        [SerializeField] private Material m_debugWheelMat;

        [SerializeField] private float m_overall = 0.15f;

        [SerializeField] private float m_tread = 0.085f;

        [SerializeField] private float m_rim = 0.25f;

        private (Mesh, Material)[] m_wheelVisuals;

        private void Start()
        {
            m_wheelVisuals = new (Mesh, Material)[m_wheelColliderSources.Length];

            for (int i = 0; i < m_wheelColliderSources.Length; i++)
            {
                var mat = new Material(m_debugWheelMat);

                var mesh = WheelMesh(m_wheelColliderSources[i].wheelPhysics.wheelRadius, m_overall, m_tread, m_rim);

                m_wheelVisuals[i] = new(mesh, mat);
            }
        }

        private void Update()
        {
            if (!m_debugWheelMat)
            {
                Debug.LogError("debug material is null");
                return;
            }

            for (int i = 0; i < m_wheelColliderSources.Length; i++)
            {
                m_wheelVisuals[i].Item2.color = m_wheelColliderSources[i].wheelPhysics.gizmoColor;

                Graphics.DrawMesh(m_wheelVisuals[i].Item1,
                    m_wheelColliderSources[i].transform.position,
                    m_wheelColliderSources[i].transform.rotation,
                    m_wheelVisuals[i].Item2, LayerMask.NameToLayer("Default"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="overall"></param>
        /// <param name="tread"></param>
        /// <param name="rim"></param>
        /// <returns></returns>
        public static Mesh WheelMesh(float radius, float overall, float tread, float rim)
        {
            var verts = new List<Vector3>();

            const int DIM = 20;

            GLUtil.CreateTrigonometricTable(DIM, out var table2D);

            var table3D = new Vector3[table2D.Length];

            for (int i = 0; i < table3D.Length; i++)
            {
                table3D[i] = new Vector3(0f, table2D[i].x, table2D[i].y);
            }

            Vector3 offset;

            offset = Vector3.left * overall;
            for (int i = 0; i < DIM; i++)
            {
                verts.Add(offset + table3D[i] * rim);
            }

            offset = Vector3.left * tread;
            for (int i = 0; i < DIM; i++)
            {
                verts.Add(offset + table3D[i] * radius);
            }

            offset = Vector3.right * overall;
            for (int i = 0; i < DIM; i++)
            {
                verts.Add(offset + table3D[i] * rim);
            }

            offset = Vector3.right * tread;
            for (int i = 0; i < DIM; i++)
            {
                verts.Add(offset + table3D[i] * radius);
            }

            verts.Add(Vector3.left * overall);
            verts.Add(Vector3.right * overall);

            var tris = new List<int>();

            for (int i = 0; i < DIM; i++)
            {
                var j = (i + 1) % DIM;

                tris.Add(i);
                tris.Add(i + DIM);
                tris.Add(j);

                tris.Add(j);
                tris.Add(i + DIM);
                tris.Add(j + DIM);

                tris.Add(i + DIM * 2);
                tris.Add(j + DIM * 3);
                tris.Add(i + DIM * 3);

                tris.Add(i + DIM * 2);
                tris.Add(j + DIM * 2);
                tris.Add(j + DIM * 3);

                tris.Add(i + DIM);
                tris.Add(j + DIM * 3);
                tris.Add(j + DIM);

                tris.Add(i + DIM);
                tris.Add(i + DIM * 3);
                tris.Add(j + DIM * 3);

                tris.Add(DIM * 2 + i);
                tris.Add(DIM * 4 + 1);
                tris.Add(DIM * 2 + j);

                tris.Add(i);
                tris.Add(j);
                tris.Add(DIM * 4);

                tris.Add(DIM * 2 + i);
                tris.Add(DIM * 4 + 1);
                tris.Add(DIM * 2 + j);
            }

            var mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
