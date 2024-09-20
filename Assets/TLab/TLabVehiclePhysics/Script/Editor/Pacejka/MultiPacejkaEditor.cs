using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.VehiclePhysics.PacejkaTool.Editor
{
    [CustomEditor(typeof(MultiPacejka))]
    public class MultiPacejkaEditor : UnityEditor.Editor
    {
        private MultiPacejka m_instance;

        private void OnEnable()
        {
            m_instance = target as MultiPacejka;
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
            var area = PacejkaEditorUtil.DrawPreviewArea();

            if (m_instance.table == null)
                return;

            var num = m_instance.graphSettings.param.x;
            var div = m_instance.graphSettings.param.y + 2;
            var range = m_instance.graphSettings.range;
            var accuracy = m_instance.graphSettings.accuracy;

            var areaXMax = range.x;
            var areaXMin = 0f;
            var areaYMax = range.y;
            var areaYMin = 0f;

            var delta = new Vector2(area.width / (areaXMax - areaXMin), area.height / (areaYMax - areaYMin));

            PacejkaEditorUtil.CreateYLable(area, area.height, areaYMin, accuracy.y);
            PacejkaEditorUtil.CreateYLable(area, 0f, areaYMax, accuracy.y);
            PacejkaEditorUtil.CreateXLabel(area, 0f, areaXMin, accuracy.x);
            PacejkaEditorUtil.CreateXLabel(area, area.width, areaXMax, accuracy.x);

            PacejkaEditorUtil.DrawGrid(area, div, accuracy.x, accuracy.y, areaXMin, areaXMax, areaYMin, areaYMax);

            for (var i = 0; i < m_instance.table.Length; i++)
            {
                var dic = m_instance.table[i];

                var pacejka = dic.pacejka;

                if (pacejka == null)
                    continue;

                Handles.color = dic.color;

                var points = new List<Vector3>();
                for (var j = 0; j <= num; j++)
                {
                    var x = (float)j / num * areaXMax;
                    var y = pacejka.Evaluate(x);
                    var point = new Vector2(area.x + delta.x * (x - areaXMin), area.yMax - delta.y * (y - areaYMin));

                    points.Add(point);
                }
                Handles.DrawAAPolyLine(2.5f, points.ToArray());
            }
        }
    }
}
