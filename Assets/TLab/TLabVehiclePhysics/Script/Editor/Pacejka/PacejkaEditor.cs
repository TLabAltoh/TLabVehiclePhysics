using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TLab.VehiclePhysics.PacejkaTool.Editor
{
    [CustomEditor(typeof(Pacejka))]
    public class PacejkaEditor : UnityEditor.Editor
    {
        private Pacejka m_instance;

        private void OnEnable()
        {
            m_instance = target as Pacejka;
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
            var area = PacejkaEditorUtil.DrawPreviewArea();

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

            Handles.color = Color.red;

            var points = new List<Vector3>();
            for (var i = 0; i <= num; i++)
            {
                var x = (float)i / num * areaXMax;
                var y = m_instance.Evaluate(x);
                var point = new Vector2(area.x + delta.x * (x - areaXMin), area.yMax - delta.y * (y - areaYMin));

                points.Add(point);
            }
            Handles.DrawAAPolyLine(2.5f, points.ToArray());
        }
    }
}
