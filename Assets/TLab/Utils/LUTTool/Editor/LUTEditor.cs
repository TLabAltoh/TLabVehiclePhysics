using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.LUTTool.Editor
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
        }

        private void DrawGraph()
        {
            var area = LUTEditorUtil.DrawPreviewArea();

            var table = m_instance.table;
            if (table == null || table.Length == 0)
                return;

            var div = m_instance.graphSettings.param + 2;
            var accuracy = m_instance.graphSettings.accuracy;

            var lutXMax = LUT.GetMax(table, LUT.Axis.X);
            var lutXMin = LUT.GetMin(table, LUT.Axis.X);
            var lutYMax = LUT.GetMax(table, LUT.Axis.Y);
            var lutYMin = LUT.GetMin(table, LUT.Axis.Y);

            var delta = new Vector2(area.width / (lutXMax - lutXMin), area.height / (lutYMax - lutYMin));

            LUTEditorUtil.CreateYLable(area, area.height, lutYMin, accuracy.y);
            LUTEditorUtil.CreateYLable(area, 0f, lutYMax, accuracy.y);
            LUTEditorUtil.CreateXLabel(area, 0f, lutXMin, accuracy.x);
            LUTEditorUtil.CreateXLabel(area, area.width, lutXMax, accuracy.x);
            LUTEditorUtil.DrawGrid(area, div, accuracy.x, accuracy.y, lutXMin, lutXMax, lutYMin, lutYMax);

            if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0)
            {
                var input = Event.current.mousePosition;
                if (table.Length > 0)
                {
                    for (var i = 0; i < table.Length; ++i)
                    {
                        var point = new Vector2(area.x + delta.x * (table[i].x - lutXMin), area.yMax - delta.y * (table[i].y - lutYMin));
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
                var input = Event.current.mousePosition;
                if (m_activeHandleIndex >= 0)
                {
                    if (!m_instance.graphSettings.lockY)
                    {
                        var lerp = (input.y - area.yMin) / area.height;
                        var y = lerp * lutYMin + (1 - lerp) * lutYMax;
                        m_instance.table[m_activeHandleIndex].y = y;
                    }
                    if (!m_instance.graphSettings.lockX)
                    {
                        var lerp = (input.x - area.xMin) / area.width;
                        var x = lerp * lutXMax + (1 - lerp) * lutXMin;
                        m_instance.table[m_activeHandleIndex].x = x;
                    }
                    EditorUtility.SetDirty(m_instance);
                }
            }
            else if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
                m_activeHandleIndex = -1;

            Handles.color = Color.red;
            if (table.Length > 0)
            {
                var points = new List<Vector3>();
                for (var i = 0; i < table.Length; ++i)
                {
                    var point = new Vector2(area.x + delta.x * (table[i].x - lutXMin), area.yMax - delta.y * (table[i].y - lutYMin));
                    Handles.DrawSolidDisc(point, Vector3.forward, 5f);
                    points.Add(point);
                }
                Handles.DrawAAPolyLine(2.5f, points.ToArray());
            }
        }
    }
}
