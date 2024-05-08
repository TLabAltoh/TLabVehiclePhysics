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

            //if (GUILayout.Button("Evaluate Test"))
            //{
            //    LogEvaluate(0.0f, 0.0f);
            //    LogEvaluate(-0.5f, 0.5f);
            //    LogEvaluate(0.5f, -0.5f);
            //    LogEvaluate(2.0f, 10.0f);
            //}
        }

        //private void LogEvaluate(float index0, float index1)
        //{
        //    var value = m_instance.Evaluate(index0, index1);
        //    Debug.Log("Evaluate Result (" + index0 + ", " + index1 + "): " + value);
        //}

        private void DrawGraph()
        {
            var area = LUTEditorUtil.DrawPreviewArea();

            if (m_instance.lutDic == null)
            {
                return;
            }

            for (var i = 0; i < m_instance.lutDic.Length; i++)
            {
                if (m_instance.lutDic[i].lut.values == null || m_instance.lutDic[i].lut.values.Length == 0)
                {
                    return;
                }
            }

            var div = m_instance.graphSettings.div + 2;
            var xAccuracy = m_instance.graphSettings.xAccuracy;
            var yAccuracy = m_instance.graphSettings.yAccuracy;

            var colorQueue = new Queue<Color>();
            var valuesQueue = new Queue<Vector2[]>();

            var lutXMax = float.MinValue;
            var lutXMin = float.MaxValue;
            var lutYMax = float.MinValue;
            var lutYMin = float.MaxValue;

            /**
             * get lut min, max value
             */
            for (int i = 0; i < m_instance.lutDic.Length; i++)
            {
                var lutDic = m_instance.lutDic[i];
                var lut = lutDic.lut;
                var color = lutDic.color;

                var values = lut.values;

                var tmpXMin = LUT.GetMin(values, 0);
                var tmpXMax = LUT.GetMax(values, 0);
                var tmpYMin = LUT.GetMin(values, 1);
                var tmpYMax = LUT.GetMax(values, 1);
                lutXMax = tmpXMax > lutXMax ? tmpXMax : lutXMax;
                lutXMin = tmpXMin < lutXMin ? tmpXMin : lutXMin;
                lutYMax = tmpYMax > lutYMax ? tmpYMax : lutYMax;
                lutYMin = tmpYMin < lutYMin ? tmpYMin : lutYMin;

                colorQueue.Enqueue(color);
                valuesQueue.Enqueue(values);
            }

            var delta = new Vector2(area.width / (lutXMax - lutXMin), area.height / (lutYMax - lutYMin));

            LUTEditorUtil.CreateYLable(area, area.height, lutYMin, yAccuracy);
            LUTEditorUtil.CreateYLable(area, 0f, lutYMax, yAccuracy);
            LUTEditorUtil.CreateXLabel(area, 0f, lutXMin, xAccuracy);
            LUTEditorUtil.CreateXLabel(area, area.width, lutXMax, xAccuracy);

            LUTEditorUtil.DrawGrid(area, div, xAccuracy, yAccuracy, lutXMin, lutXMax, lutYMin, lutYMax);

            /**
             * plot data
             */
            var queueLength = valuesQueue.Count;
            for (int i = 0; i < queueLength; i++)
            {
                var values = valuesQueue.Dequeue();
                var color = colorQueue.Dequeue();

                Handles.color = color;
                if (values.Length > 0)
                {
                    var points = new List<Vector3>();
                    for (var j = 0; j < values.Length; ++j)
                    {
                        var point = new Vector2(area.x + delta.x * (values[j].x - lutXMin), area.yMax - delta.y * (values[j].y - lutYMin));

                        Handles.DrawSolidDisc(point, Vector3.forward, 5f);

                        points.Add(point);
                    }
                    Handles.DrawAAPolyLine(2.5f, points.ToArray());
                }
            }
        }
    }
}
