using System.Collections.Generic;
using UnityEngine;

namespace TLab
{
    public static class GLUtil
    {
        public const int LINE_WIDTH = 8;

        public const int LINES_WIDTH = 9;

        private static int m_mode = 0;

        private static int m_dim = 0;

        private static float m_width = 0;

        private static bool m_closed = false;

        private static bool m_screenSpace = false;

        private static Queue<Vector3> m_verts = new Queue<Vector3>();

        private static float[] m_sin = null;

        private static float[] m_cos = null;

        public static void CreateTrigonometricTable(int dim, out float[] sin, out float[] cos)
        {
            if ((m_sin == null) || (m_sin.Length != dim))
            {
                m_sin = new float[dim];
                m_cos = new float[dim];

                for (int i = 0; i < dim; i++)
                {
                    float theta = i / (float)dim * Mathf.PI * 2;
                    m_sin[i] = Mathf.Sin(theta);
                    m_cos[i] = Mathf.Cos(theta);
                }
            }

            sin = m_sin;
            cos = m_cos;
        }

        public static void Begin(int mode, float width = 0.0f, bool closed = false, int dim = 10)
        {
            m_verts.Clear();

            m_mode = mode;

            m_dim = dim;

            m_width = width;

            m_closed = closed;
        }

        public static void PushMatrix() => GL.PushMatrix();

        public static void PopMatrix() => GL.PopMatrix();

        public static void WorldToScreenVertex(Vector3 vert, in Camera camera)
        {
            m_screenSpace = true;

            Vector4 projected = new Vector4(vert.x, vert.y, vert.z, 1.0f);

            projected = camera.projectionMatrix * camera.worldToCameraMatrix * projected;

            projected /= projected.w;

            // -1 ~ 1 to 0 ~ 1
            projected = projected * 0.5f + Vector4.one * 0.5f;

            m_verts.Enqueue(projected);
        }

        public static void ScreenVertex(Vector3 vert)
        {
            m_screenSpace = true;

            m_verts.Enqueue(vert);
        }

        public static void WorldVertex(Vector3 vert)
        {
            m_screenSpace = false;

            m_verts.Enqueue(vert);
        }

        private static void DrawLine(Vector3 vert0, Vector3 vert1)
        {
            Vector3 normal, vertOL, vertOR, vertCL, vertCR;

            normal = vert0 - vert1;
            normal = new Vector2(normal.y, -normal.x);  // +90 degrees clockwise in xy axis
            normal = normal.normalized;

            vertCL = vert0 - normal * m_width;
            vertCR = vert0 + normal * m_width;

            vertOL = vert1 - normal * m_width;
            vertOR = vert1 + normal * m_width;

            GL.Vertex(vertOL);
            GL.Vertex(vertCL);
            GL.Vertex(vertCR);

            GL.Vertex(vertCR);
            GL.Vertex(vertOR);
            GL.Vertex(vertOL);
        }

        private static void DrawCircle(Vector3 vert)
        {
            CreateTrigonometricTable(m_dim, out float[] sin, out float[] cos);

            Vector3 offset0 = new Vector3(cos[0] * m_width, sin[0] * m_width), offset1 = Vector3.zero;

            for (int i = 1; i < m_dim; i++)
            {
                offset1.x = cos[i] * m_width;
                offset1.y = sin[i] * m_width;

                GL.Vertex(vert);
                GL.Vertex(vert + offset1);
                GL.Vertex(vert + offset0);

                offset0 = offset1;
            }

            offset1.x = cos[0] * m_width;
            offset1.y = sin[0] * m_width;

            GL.Vertex(vert);
            GL.Vertex(vert + offset1);
            GL.Vertex(vert + offset0);
        }

        private static void DrawShape()
        {
            if (m_screenSpace)
            {
                GL.LoadOrtho();
            }

            Vector3[] verts = m_verts.ToArray();

            m_verts.Clear();

            if (m_mode == LINE_WIDTH)
            {
                if (verts.Length > 2)
                {
                    GL.Begin(GL.TRIANGLES);
                    {
                        for (int i = 0; i < verts.Length - 1; i++)
                        {
                            DrawCircle(verts[i]);
                            DrawLine(verts[i + 0], verts[i + 1]);
                        }

                        DrawCircle(verts[verts.Length - 1]);

                        if (m_closed)
                        {
                            DrawLine(verts[verts.Length - 1], verts[0]);
                        }
                    }
                    GL.End();
                }
                else if (verts.Length == 2)
                {
                    DrawCircle(verts[0]);

                    DrawLine(verts[0], verts[1]);

                    DrawCircle(verts[1]);
                }
            }
            else if (m_mode == LINES_WIDTH)
            {
                GL.Begin(GL.TRIANGLES);
                {
                    for (int i = 0; i < verts.Length - 1; i += 2)
                    {
                        DrawCircle(verts[i + 0]);

                        DrawLine(verts[i + 0], verts[i + 1]);

                        DrawCircle(verts[i + 1]);
                    }
                }
                GL.End();
            }
            else
            {
                GL.Begin(m_mode);
                {
                    for (int i = 0; i < verts.Length; i++)
                    {
                        GL.Vertex(verts[i]);
                    }
                }
                GL.End();
            }
        }

        public static void End()
        {
            DrawShape();

            m_mode = 0;

            m_width = 0.0f;

            m_screenSpace = false;
        }
    }
}
