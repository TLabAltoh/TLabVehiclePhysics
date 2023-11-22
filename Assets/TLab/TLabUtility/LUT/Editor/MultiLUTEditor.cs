using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.Editor
{
    [CustomEditor(typeof(MultiLUT))]
    public class MultiLUTEditor : UnityEditor.Editor
    {
        private MultiLUT m_instance;
        private bool m_draw = false;

        private GUIStyle m_ylabelStyle = null;
        private readonly Vector2 LABEL_SIZE = new Vector2(100f, 50f);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_instance == null)
            {
                m_instance = target as MultiLUT;
            }

            if (GUILayout.Button("DrawGraph"))
            {
                m_draw = !m_draw;
            }

            if (m_draw)
            {
                EditorGUILayout.Space(20);
                DrawGraph();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Evaluate Test"))
            {
                LogEvaluate(0.0f, 0.0f);
                LogEvaluate(-0.5f, 0.5f);
                LogEvaluate(0.5f, -0.5f);
                LogEvaluate(2.0f, 10.0f);
            }
        }

        private void LogEvaluate(float index0, float index1)
        {
            var value = m_instance.Evaluate(index0, index1);
            Debug.Log("Evaluate Result (" + index0 + ", " + index1 + "): " + value);
        }

        private void CreateXLabel(Rect area, float areaYMax, float v, float x)
        {
            var vx = new Vector2(area.x + x - 10f, areaYMax + 1f);

            GUI.Label(new Rect(vx, LABEL_SIZE), ((int)v).ToString());
        }

        private void CreateYLable(Rect area, float v, float y)
        {
            Vector2 vy = new Vector2(area.x - 110f, area.y + y - 10f);

            GUI.Label(new Rect(vy, LABEL_SIZE), ((int)v).ToString(), m_ylabelStyle);
        }

        private void DrawGraph()
        {
            var margin = 0.8f;
            var xoffset = Screen.width * (1 - margin) * 0.25f;

            var area = GUILayoutUtility.GetRect(Screen.width * margin, 250f, GUILayout.ExpandWidth(false));
            area.xMax += xoffset;
            area.x += xoffset;

            var areaXMax = area.xMax;
            var areaYMax = area.yMax - 50f;
            var areaXSize = areaXMax - area.xMin;
            var areaYSize = areaYMax - area.yMin;

            Handles.color = Color.white;
            Handles.DrawLine(new Vector2(area.x, area.y), new Vector2(area.x, areaYMax));
            Handles.DrawLine(new Vector2(area.x, area.y), new Vector2(areaXMax, area.y));
            Handles.DrawLine(new Vector2(areaXMax, area.y), new Vector2(areaXMax, areaYMax));
            Handles.DrawLine(new Vector2(area.x, areaYMax), new Vector2(areaXMax, areaYMax));

            if (m_instance.lutDic == null)
            {
                return;
            }

            if (m_ylabelStyle == null)
            {
                m_ylabelStyle = new GUIStyle(EditorStyles.label);
                m_ylabelStyle.alignment = TextAnchor.UpperRight;
            }

            var div = m_instance.div + 2;

            var xMax = -float.MaxValue;
            var xMin = float.MaxValue;
            var yMax = -float.MaxValue;
            var yMin = float.MaxValue;
            var valuesQueue = new Queue<Vector2[]>();
            var colorQueue = new Queue<Color>();

            for (int i = 0; i < m_instance.lutDic.Length; i++)
            {
                var lutDic = m_instance.lutDic[i];
                var lut = lutDic.lut;
                var color = lutDic.color;

                var values = lut.values;

                var tmpXMin = values[0].x;
                var tmpXMax = values[values.Length - 1].x;
                var tmpYMax = LUT.GetMax(values, 1);
                var tmpYMin = LUT.GetMin(values, 1);
                xMax = tmpXMax > xMax ? tmpXMax : xMax;
                xMin = tmpXMin < xMin ? tmpXMin : xMin;
                yMax = tmpYMax > yMax ? tmpYMax : yMax;
                yMin = tmpYMin < yMin ? tmpYMin : yMin;

                valuesQueue.Enqueue(values);
                colorQueue.Enqueue(color);
            }

            CreateYLable(area, yMax, 0f);
            CreateYLable(area, yMin, areaYSize);
            CreateXLabel(area, areaYMax, xMin, 0f);
            CreateXLabel(area, areaYMax, xMax, areaXSize);

            for (int xi = 1; xi < div - 1; ++xi)
            {
                var lerp = (float)xi / (div - 1);
                var x = lerp * areaXSize;
                var v = xMin * (1f - lerp) + xMax * lerp;

                // draw grid
                Handles.DrawLine(new Vector2(area.x + x, area.y), new Vector2(area.x + x, areaYMax));

                if ((int)v != v)
                {
                    continue;
                }

                CreateXLabel(area, areaYMax, v, x);
            }

            for (int yi = 1; yi < div - 1; ++yi)
            {
                var lerp = (float)yi / (div - 1);
                var y = areaYSize * (1f - lerp);
                var v = yMin * (1f - lerp) + yMax * lerp;

                // draw grid
                Handles.DrawLine(new Vector2(area.x, area.y + y), new Vector2(areaXMax, area.y + y));

                if ((int)v != v)
                {
                    continue;
                }

                CreateYLable(area, v, y);
            }

            // plot data
            var queueLength = valuesQueue.Count;
            for (int i = 0; i < queueLength; i++)
            {
                var values = valuesQueue.Dequeue();
                var color = colorQueue.Dequeue();

                Handles.color = color;
                if (values.Length > 0)
                {
                    var points = new List<Vector3>();
                    var dx = areaXSize / xMax;
                    var dy = areaYSize / yMax;
                    for (var j = 0; j < values.Length; ++j)
                    {
                        var x = area.x + dx * values[j].x;
                        var y = areaYMax - dy * values[j].y;
                        points.Add(new Vector2(x, y));
                    }
                    Handles.DrawAAPolyLine(2.5f, points.ToArray());
                }
            }
        }
    }
}
