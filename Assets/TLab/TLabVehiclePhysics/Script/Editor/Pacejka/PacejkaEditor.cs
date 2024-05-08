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

        /// <summary>
        /// 
        /// </summary>
        private void DrawGraph()
        {
            var area = PacejkaEditorUtil.DrawPreviewArea();

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
