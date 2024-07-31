using System.Collections.Generic;
using UnityEngine;

namespace TLab
{
    public static class GLUtil
    {
        public static Queue<Vector3> m_verts = new Queue<Vector3>();

        public static void AddVertex(Vector3 vert)
        {
            m_verts.Enqueue(vert);
        }

        public static void ClearVertex()
        {
            m_verts.Clear();
        }

        public static void CreateTrigonometricTable(int dim, out float[] sin, out float[] cos)
        {
            sin = new float[dim];
            cos = new float[dim];

            for (int i = 0; i < dim; i++)
            {
                float theta = i / (float)dim * Mathf.PI * 2;
                sin[i] = Mathf.Sin(theta);
                cos[i] = Mathf.Cos(theta);
            }
        }

        public static void CreateTrigonometricTable(int dim, out Vector2[] table)
        {
            table = new Vector2[dim];

            for (int i = 0; i < dim; i++)
            {
                float theta = i / (float)dim * Mathf.PI * 2;
                table[i] = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
            }
        }

        public static Vector2 WorldToScreenVertex(Vector3 vert, in Camera camera)
        {
            Vector4 projected = new Vector4(vert.x, vert.y, vert.z, 1.0f);

            projected = camera.projectionMatrix * camera.worldToCameraMatrix * projected;

            projected /= projected.w;

            // -1 ~ 1 to 0 ~ 1
            projected = projected * 0.5f + Vector4.one * 0.5f;

            return projected;
        }

        private static void DrawCircle(Vector3 vert, float width, int dim)
        {
            CreateTrigonometricTable(dim, out float[] sin, out float[] cos);

            Vector3 offset0 = new Vector3(cos[0] * width, sin[0] * width), offset1 = Vector3.zero;

            for (int i = 1; i < dim; i++)
            {
                offset1.x = cos[i] * width;
                offset1.y = sin[i] * width;

                GL.Vertex(vert);
                GL.Vertex(vert + offset1);
                GL.Vertex(vert + offset0);

                offset0 = offset1;
            }

            offset1.x = cos[0] * width;
            offset1.y = sin[0] * width;

            GL.Vertex(vert);
            GL.Vertex(vert + offset1);
            GL.Vertex(vert + offset0);
        }

        private static void DrawLine(Vector3 vert0, Vector3 vert1, float width)
        {
            Vector3 normal, vertOL, vertOR, vertCL, vertCR;

            normal = vert0 - vert1;
            normal = new Vector2(normal.y, -normal.x);  // +90 degrees clockwise in xy axis
            normal = normal.normalized;

            vertCL = vert0 - normal * width;
            vertCR = vert0 + normal * width;

            vertOL = vert1 - normal * width;
            vertOR = vert1 + normal * width;

            GL.Vertex(vertOL);
            GL.Vertex(vertCL);
            GL.Vertex(vertCR);

            GL.Vertex(vertCR);
            GL.Vertex(vertOR);
            GL.Vertex(vertOL);
        }

        public static void DrawLineWidth(
            Camera camera, float width = 0.0f, bool closed = false, int dim = 10)
        {
            Vector3[] verts = m_verts.ToArray();

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = WorldToScreenVertex(verts[i], camera);
            }

            GL.PushMatrix();
            {
                GL.LoadOrtho();

                GL.Begin(GL.TRIANGLES);
                {
                    if (verts.Length > 2)
                    {
                        for (int i = 0; i < verts.Length - 1; i++)
                        {
                            DrawCircle(verts[i], width, dim);
                            DrawLine(verts[i + 0], verts[i + 1], width);
                        }

                        DrawCircle(verts[verts.Length - 1], width, dim);

                        if (closed)
                        {
                            DrawLine(verts[verts.Length - 1], verts[0], width);
                        }
                    }
                    else if (verts.Length == 2)
                    {
                        DrawCircle(verts[0], width, dim);

                        DrawLine(verts[0], verts[1], width);

                        DrawCircle(verts[1], width, dim);
                    }
                }
                GL.End();
            }
            GL.PopMatrix();
        }

        public static void DrawLinesWidth(
            Camera camera, float width = 0.0f, int dim = 10)
        {
            Vector3[] verts = m_verts.ToArray();

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = WorldToScreenVertex(verts[i], camera);
            }

            GL.PushMatrix();
            {
                GL.LoadOrtho();

                GL.Begin(GL.TRIANGLES);
                {
                    for (int i = 0; i < verts.Length - 1; i += 2)
                    {
                        DrawCircle(verts[i + 0], width, dim);

                        DrawLine(verts[i + 0], verts[i + 1], width);

                        DrawCircle(verts[i + 1], width, dim);
                    }
                }
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}
