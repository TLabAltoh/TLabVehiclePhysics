using UnityEngine;

namespace TLab.MeshEngine
{
    public class MeshArc : MeshElement
    {
        [SerializeField] private int m_resolusion = 20;
        [SerializeField] private float m_width = 0.1f;
        [SerializeField] private float m_radius = 1.0f;
        [SerializeField] private float m_thickness = 0.1f;

        public override void GetMeshInfo(
            out Vector3[] vertices, out Vector2[] uv, out int[] triangles)
        {
            vertices = new Vector3[m_resolusion * 2 * 2];
            uv = new Vector2[vertices.Length];
            triangles = new int[(vertices.Length - 2) * 3];

            for (int i = 0; i < m_resolusion; i++)
            {
                var fill = (float)i / (m_resolusion - 1);
                var theta = fill * Mathf.PI * 2.0f;
                var sin = Mathf.Sin(theta);
                var cos = Mathf.Cos(theta);

                var pos = new Vector3(cos, sin) * m_radius;

                var dir = pos.normalized * m_width;

                var exc = new Vector3[] { Vector3.forward, Vector3.back };

                var order = new int[][]
                {
                    new int[] {1, 0, 2, 3, 1, 2},
                    new int[] {0, 1, 2, 1, 3, 2}
                };

                for (int j = 0; j < 2; j++) // back, forward
                {
                    var exclude = exc[j] * m_thickness;

                    var offset0 = i * 2;
                    var offset1 = j * (m_resolusion * 2);

                    var offset = offset0 + offset1;

                    vertices[offset + 0] = pos + dir + exclude;
                    vertices[offset + 1] = pos - dir + exclude;

                    uv[offset + 0] = new Vector2(0.0f, fill);
                    uv[offset + 1] = new Vector2(1.0f, fill);

                    if (i < m_resolusion - 1)
                    {
                        var tmp = i * 6 + (m_resolusion * 6) * j;

                        triangles[tmp + 0] = offset + order[j][0];
                        triangles[tmp + 1] = offset + order[j][1];
                        triangles[tmp + 2] = offset + order[j][2];

                        triangles[tmp + 3] = offset + order[j][3];
                        triangles[tmp + 4] = offset + order[j][4];
                        triangles[tmp + 5] = offset + order[j][5];
                    }
                }
            }
        }
    }
}
