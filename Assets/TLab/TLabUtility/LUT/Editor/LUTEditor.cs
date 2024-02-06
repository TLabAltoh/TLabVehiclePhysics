using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.Editor
{
    [CustomEditor(typeof(LUT))]
    public class LUTEditor : UnityEditor.Editor
    {
        private LUT m_instance;

        private GUIStyle m_ylabelStyle = null;
        private readonly Vector2 LABEL_SIZE = new Vector2(100f, 50f);

        private void OnEnable()
        {
            m_instance = target as LUT;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);
            DrawGraph();
            EditorGUILayout.Space(20);

            if (GUILayout.Button("Evaluate Test"))
            {
                LogEvaluate(0.0f);
                LogEvaluate(0.5f);
                LogEvaluate(-0.5f);
                LogEvaluate(10.0f);
            }
        }

        private void LogEvaluate(float index)
        {
            var value = m_instance.Evaluate(index);
            Debug.Log("Evaluate Result: " + value);
        }

        private float Floor(float value, int i) => Mathf.Floor(value * i) / i;

        private void CreateXLabel(Rect area, float areaYMax, float v, float x, int i)
        {
            var vx = new Vector2(area.x + x - 10f, areaYMax + 1f);

            GUI.Label(new Rect(vx, LABEL_SIZE), Floor(v, i).ToString());
        }

        private void CreateYLable(Rect area, float v, float y, int i)
        {
            Vector2 vy = new Vector2(area.x - 110f, area.y + y - 10f);

            GUI.Label(new Rect(vy, LABEL_SIZE), Floor(v, i).ToString(), m_ylabelStyle);
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

            if (m_instance.values == null)
            {
                return;
            }

            if (m_ylabelStyle == null)
            {
                m_ylabelStyle = new GUIStyle(EditorStyles.label);
                m_ylabelStyle.alignment = TextAnchor.UpperRight;
            }

            var values = m_instance.values;
            var div = m_instance.div + 2;
            var xAccuracy = m_instance.xAccuracy;
            var yAccuracy = m_instance.yAccuracy;

            var xMax = values[values.Length - 1].x;
            var xMin = values[0].x;
            var yMax = LUT.GetMax(values, 1);
            var yMin = LUT.GetMin(values, 1);

            CreateYLable(area, yMax, 0f, yAccuracy);
            CreateYLable(area, yMin, areaYSize, yAccuracy);
            CreateXLabel(area, areaYMax, xMin, 0f, xAccuracy);
            CreateXLabel(area, areaYMax, xMax, areaXSize, xAccuracy);

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

                CreateXLabel(area, areaYMax, v, x, xAccuracy);
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

                CreateYLable(area, v, y, yAccuracy);
            }

            // plot data
            Handles.color = Color.red;
            if (values.Length > 0)
            {
                var points = new List<Vector3>();
                var dx = areaXSize / (xMax - xMin);
                var dy = areaYSize / (yMax - yMin);
                for (var i = 0; i < values.Length; ++i)
                {
                    var x = area.x + dx * (values[i].x - xMin);
                    var y = areaYMax - dy * (values[i].y - yMin);
                    points.Add(new Vector2(x, y));
                }
                Handles.DrawAAPolyLine(2.5f, points.ToArray());
            }
        }
    }
}
