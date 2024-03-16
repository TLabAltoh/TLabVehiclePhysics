using System.Collections.Generic;
using UnityEngine;

namespace TLab
{
    public static class GLUtil
    {
        public const int LINE_WIDTH = 8;

        public const int LINES_WIDTH = 9;

        private static int m_mode = 0;

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
                    float theta = i / 20f * Mathf.PI * 2;
                    m_sin[i] = Mathf.Sin(theta);
                    m_cos[i] = Mathf.Cos(theta);
                }
            }

            sin = m_sin;
            cos = m_cos;
        }

        public static void Begin(int mode, float width = 0.0f, bool closed = false)
        {
            m_verts.Clear();

            m_mode = mode;

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
                Vector3 normal, vert = verts[0], vertOL, vertOR, vertCL, vertCR;

                if (verts.Length > 2)
                {
                    if (m_closed)
                    {
                        normal = (vert - verts[verts.Length - 1]).normalized + (verts[1] - vert).normalized;
                        normal = normal.normalized;
                    }
                    else
                    {
                        normal = (verts[1] - vert).normalized;
                    }

                    normal = new Vector2(normal.y, -normal.x);  // +90 degrees clockwise

                    vertOL = vert - normal * m_width;
                    vertOR = vert + normal * m_width;

                    GL.Begin(GL.TRIANGLES);
                    {
                        for (int i = 1; i < verts.Length - 1; i++)
                        {
                            vert = verts[i];

                            normal = (vert - verts[i - 1]).normalized + (verts[i + 1] - vert).normalized;
                            normal = normal.normalized;
                            normal = new Vector2(normal.y, -normal.x);

                            vertCL = vert - normal * m_width;
                            vertCR = vert + normal * m_width;

                            GL.Vertex(vertOL);
                            GL.Vertex(vertCL);
                            GL.Vertex(vertCR);

                            GL.Vertex(vertCR);
                            GL.Vertex(vertOR);
                            GL.Vertex(vertOL);

                            vertOL = vertCL;
                            vertOR = vertCR;
                        }

                        vert = verts[verts.Length - 1];

                        normal = (vert - verts[verts.Length - 2]).normalized + (verts[0] - vert).normalized;
                        normal = normal.normalized;
                        normal = new Vector2(normal.y, -normal.x);

                        vertCL = vert - normal * m_width;
                        vertCR = vert + normal * m_width;

                        GL.Vertex(vertOL);
                        GL.Vertex(vertCL);
                        GL.Vertex(vertCR);

                        GL.Vertex(vertCR);
                        GL.Vertex(vertOR);
                        GL.Vertex(vertOL);

                        vertOL = vertCL;
                        vertOR = vertCR;

                        if (m_closed)
                        {
                            vert = verts[0];

                            normal = (vert - verts[verts.Length - 1]).normalized + (verts[1] - vert).normalized;
                            normal = normal.normalized;
                            normal = new Vector2(normal.y, -normal.x);

                            vertCL = vert - normal * m_width;
                            vertCR = vert + normal * m_width;

                            GL.Vertex(vertOL);
                            GL.Vertex(vertCL);
                            GL.Vertex(vertCR);

                            GL.Vertex(vertCR);
                            GL.Vertex(vertOR);
                            GL.Vertex(vertOL);
                        }
                    }
                    GL.End();
                }
                else if (verts.Length == 2)
                {
                    normal = (verts[1] - vert).normalized;
                    normal = new Vector2(normal.y, -normal.x);  // +90 degrees clockwise

                    vertOL = verts[0] - normal * m_width;
                    vertOR = verts[0] + normal * m_width;

                    vertCL = verts[1] - normal * m_width;
                    vertCR = verts[1] + normal * m_width;

                    GL.Vertex(vertOL);
                    GL.Vertex(vertCL);
                    GL.Vertex(vertCR);

                    GL.Vertex(vertCR);
                    GL.Vertex(vertOR);
                    GL.Vertex(vertOL);
                }
            }
            else if (m_mode == LINES_WIDTH)
            {
                Vector3 normal, vert, vertOL, vertOR, vertCL, vertCR;

                GL.Begin(GL.TRIANGLES);
                {
                    for (int i = 0; i < verts.Length - 1; i += 2)
                    {
                        vert = verts[i];

                        normal = (verts[i + 1] - vert).normalized;
                        normal = new Vector2(normal.y, -normal.x);  // +90 degrees clockwise

                        vertOL = verts[i] - normal * m_width;
                        vertOR = verts[i] + normal * m_width;

                        vertCL = verts[i + 1] - normal * m_width;
                        vertCR = verts[i + 1] + normal * m_width;

                        GL.Vertex(vertOL);
                        GL.Vertex(vertCL);
                        GL.Vertex(vertCR);

                        GL.Vertex(vertCR);
                        GL.Vertex(vertOR);
                        GL.Vertex(vertOL);
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
