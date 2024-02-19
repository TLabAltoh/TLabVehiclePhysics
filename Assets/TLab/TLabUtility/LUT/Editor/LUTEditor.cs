using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.Editor
{
    [CustomEditor(typeof(LUT))]
    public class LUTEditor : UnityEditor.Editor
    {
        private LUT m_instance;

        private int m_activeHandleIndex = -1;

        private void OnEnable()
        {
            m_instance = target as LUT;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);
            DrawGraph();
            EditorGUILayout.Space(40);

            //if (GUILayout.Button("Evaluate Test"))
            //{
            //    LogEvaluate(0.0f);
            //    LogEvaluate(0.5f);
            //    LogEvaluate(-0.5f);
            //    LogEvaluate(10.0f);
            //}
        }

        //private void LogEvaluate(float index)
        //{
        //    var value = m_instance.Evaluate(index);
        //    Debug.Log("Evaluate Result: " + value);
        //}

        private void DrawGraph()
        {
            var area = LUTEditorUtil.DrawPreviewArea();

            if (m_instance.values == null || m_instance.values.Length == 0)
            {
                return;
            }

            var values = m_instance.values;
            var div = m_instance.div + 2;
            var xAccuracy = m_instance.xAccuracy;
            var yAccuracy = m_instance.yAccuracy;

            var lutXMax = LUT.GetMax(values, 0);
            var lutXMin = LUT.GetMin(values, 0);
            var lutYMax = LUT.GetMax(values, 1);
            var lutYMin = LUT.GetMin(values, 1);

            var delta = new Vector2(area.width / (lutXMax - lutXMin), area.height / (lutYMax - lutYMin));

            LUTEditorUtil.CreateYLable(area, area.height, lutYMin, yAccuracy);
            LUTEditorUtil.CreateYLable(area, 0f, lutYMax, yAccuracy);
            LUTEditorUtil.CreateXLabel(area, 0f, lutXMin, xAccuracy);
            LUTEditorUtil.CreateXLabel(area, area.width, lutXMax, xAccuracy);

            LUTEditorUtil.DrawGrid(area, div, xAccuracy, yAccuracy, lutXMin, lutXMax, lutYMin, lutYMax);

            /**
             * handle drag event
             */
            if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 input = Event.current.mousePosition;

                if (values.Length > 0)
                {
                    for (var i = 0; i < values.Length; ++i)
                    {
                        var point = new Vector2(area.x + delta.x * (values[i].x - lutXMin), area.yMax - delta.y * (values[i].y - lutYMin));

                        if (Vector2.Distance(point, input) < 5)
                        {
                            m_activeHandleIndex = (m_activeHandleIndex == -1) ? i : m_activeHandleIndex;

                            break;
                        }
                    }
                }
            }
            else if (Event.current.rawType == EventType.MouseDrag && Event.current.button == 0)
            {
                Vector2 input = Event.current.mousePosition;

                if (m_activeHandleIndex >= 0)
                {
                    if (!m_instance.fixY)
                    {
                        var lerp = (input.y - area.yMin) / area.height;

                        var y = lerp * lutYMin + (1 - lerp) * lutYMax;

                        m_instance.values[m_activeHandleIndex].y = y;
                    }

                    if (!m_instance.fixX)
                    {
                        var lerp = (input.x - area.xMin) / area.width;

                        var x = lerp * lutXMax + (1 - lerp) * lutXMin;

                        m_instance.values[m_activeHandleIndex].x = x;
                    }

                    EditorUtility.SetDirty(m_instance);
                }
            }
            else if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
            {
                m_activeHandleIndex = -1;
            }

            /**
             * plot data and handle drag event
             */
            Handles.color = Color.red;

            if (values.Length > 0)
            {
                var points = new List<Vector3>();
                for (var i = 0; i < values.Length; ++i)
                {
                    var point = new Vector2(area.x + delta.x * (values[i].x - lutXMin), area.yMax - delta.y * (values[i].y - lutYMin));

                    Handles.DrawSolidDisc(point, Vector3.forward, 5f);

                    points.Add(point);
                }
                Handles.DrawAAPolyLine(2.5f, points.ToArray());
            }
        }
    }
}
