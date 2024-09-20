using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.LUTTool.Editor
{
    [CustomEditor(typeof(MultiLUT))]
    public class MultiLUTEditor : UnityEditor.Editor
    {
        private MultiLUT m_instance;

        private void OnEnable()
        {
            m_instance = target as MultiLUT;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(20);
            DrawGraph();
            EditorGUILayout.Space(20);
        }

        private void DrawGraph()
        {
            var area = LUTEditorUtil.DrawPreviewArea();

            if (m_instance.table == null)
                return;

            for (var i = 0; i < m_instance.table.Length; i++)
            {
                var lut = m_instance.table[i].lut;
                if (lut == null || lut.table == null || lut.table.Length == 0)
                    return;
            }

            var div = m_instance.graphSettings.param + 2;
            var accuracy = m_instance.graphSettings.accuracy;

            var colorQueue = new Queue<Color>();
            var tableQueue = new Queue<Vector2[]>();

            var lutXMax = float.MinValue;
            var lutXMin = float.MaxValue;
            var lutYMax = float.MinValue;
            var lutYMin = float.MaxValue;

            for (int i = 0; i < m_instance.table.Length; i++)
            {
                var table0 = m_instance.table[i];
                var lut = table0.lut;
                var color = table0.color;
                var table1 = lut.table;

                var tmpXMin = LUT.GetMin(table1, LUT.Axis.X);
                var tmpXMax = LUT.GetMax(table1, LUT.Axis.X);
                var tmpYMin = LUT.GetMin(table1, LUT.Axis.Y);
                var tmpYMax = LUT.GetMax(table1, LUT.Axis.Y);
                lutXMax = tmpXMax > lutXMax ? tmpXMax : lutXMax;
                lutXMin = tmpXMin < lutXMin ? tmpXMin : lutXMin;
                lutYMax = tmpYMax > lutYMax ? tmpYMax : lutYMax;
                lutYMin = tmpYMin < lutYMin ? tmpYMin : lutYMin;

                colorQueue.Enqueue(color);
                tableQueue.Enqueue(table1);
            }

            var delta = new Vector2(area.width / (lutXMax - lutXMin), area.height / (lutYMax - lutYMin));

            LUTEditorUtil.CreateYLable(area, area.height, lutYMin, accuracy.y);
            LUTEditorUtil.CreateYLable(area, 0f, lutYMax, accuracy.y);
            LUTEditorUtil.CreateXLabel(area, 0f, lutXMin, accuracy.x);
            LUTEditorUtil.CreateXLabel(area, area.width, lutXMax, accuracy.x);
            LUTEditorUtil.DrawGrid(area, div, accuracy.x, accuracy.y, lutXMin, lutXMax, lutYMin, lutYMax);

            var queueLength = tableQueue.Count;
            for (var i = 0; i < queueLength; i++)
            {
                var table = tableQueue.Dequeue();
                var color = colorQueue.Dequeue();

                Handles.color = color;
                if (table.Length > 0)
                {
                    var points = new List<Vector3>();
                    for (var j = 0; j < table.Length; ++j)
                    {
                        var point = new Vector2(area.x + delta.x * (table[j].x - lutXMin), area.yMax - delta.y * (table[j].y - lutYMin));
                        Handles.DrawSolidDisc(point, Vector3.forward, 5f);
                        points.Add(point);
                    }
                    Handles.DrawAAPolyLine(2.5f, points.ToArray());
                }
            }
        }
    }
}
