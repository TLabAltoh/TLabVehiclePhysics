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

            //if (GUILayout.Button("Evaluate Test"))
            //{
            //    LogEvaluate(0f, 10f);
            //    LogEvaluate(10f, 50f);
            //    LogEvaluate(20f, 70f);
            //    LogEvaluate(0f, 99f);
            //}
        }

        //private void LogEvaluate(float index0, float index1)
        //{
        //    var value = m_instance.Evaluate(index0, index1);
        //    Debug.Log("Evaluate Result (" + index0 + ", " + index1 + "): " + value);
        //}

        private void DrawGraph()
        {
            var area = PacejkaEditorUtil.DrawPreviewArea();

            if (m_instance.pacejkaDic == null)
            {
                return;
            }

            var num = m_instance.graphSettings.num;
            var div = m_instance.graphSettings.div + 2;
            var xAccuracy = m_instance.graphSettings.xAccuracy;
            var yAccuracy = m_instance.graphSettings.yAccuracy;

            var areaXMax = m_instance.graphSettings.xrange;
            var areaXMin = 0f;
            var areaYMax = m_instance.graphSettings.yrange;
            var areaYMin = 0f;

            var delta = new Vector2(area.width / (areaXMax - areaXMin), area.height / (areaYMax - areaYMin));

            PacejkaEditorUtil.CreateYLable(area, area.height, areaYMin, yAccuracy);
            PacejkaEditorUtil.CreateYLable(area, 0f, areaYMax, yAccuracy);
            PacejkaEditorUtil.CreateXLabel(area, 0f, areaXMin, xAccuracy);
            PacejkaEditorUtil.CreateXLabel(area, area.width, areaXMax, xAccuracy);

            PacejkaEditorUtil.DrawGrid(area, div, xAccuracy, yAccuracy, areaXMin, areaXMax, areaYMin, areaYMax);

            // 
            // plot data
            //

            for (int i = 0; i < m_instance.pacejkaDic.Length; i++)
            {
                var dic = m_instance.pacejkaDic[i];

                var pacejka = dic.pacejka;

                if (pacejka == null)
                {
                    continue;
                }

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
