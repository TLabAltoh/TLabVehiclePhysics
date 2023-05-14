using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(TLabLUT))]
public class TLabLUTEditor : Editor
{
    private TLabLUT instance;
    private bool draw = false;

    private static GUIStyle ylabelStyle = null;
    private static readonly Vector2 labelSize = new Vector2(100f, 50f);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(instance == null)
        {
            instance = target as TLabLUT;
        }

        if (GUILayout.Button("DrawGraph"))
        {
            draw = !draw;
        }

        if (draw)
        {
            EditorGUILayout.Space();
            DrawGraph();
            EditorGUILayout.Space();
        }
    }

    private void DrawGraph()
    {
        float ratio = 0.8f;
        float xoffset = Screen.width * (1 - ratio) * 0.25f;

        Rect area = GUILayoutUtility.GetRect(Screen.width * ratio, 200f, GUILayout.ExpandWidth(false));
        area.xMax += xoffset;
        area.x += xoffset;

        if(instance.indexs == null || instance.values == null)
        {
            return;
        }

        int xdiv = instance.indexs.Length;
        int ydiv = instance.values.Length;

        Handles.color = Color.white;

        Handles.DrawLine(new Vector2(area.xMax, area.y), new Vector2(area.xMax, area.yMax));
        Handles.DrawLine(new Vector2(area.x, area.yMax), new Vector2(area.xMax, area.yMax));

        for (int xi = 0; xi <= xdiv - 1; ++xi)
        {
            var x = (area.width / instance.indexs.Max()) * instance.indexs[xi];

            // draw grid
            Handles.DrawLine(new Vector2(area.x + x, area.y), new Vector2(area.x + x, area.yMax));

            // add xlabel
            Vector2 vx = new Vector2(area.x + x - 10f, area.yMax + 1f);

            GUI.Label(new Rect(vx, labelSize), Convert.ToString(instance.indexs[xi]));
        }

        for (int yi = 0; yi <= ydiv - 1; ++yi)
        {
            var y = area.height * (1 - instance.values[(ydiv - 1) - yi] / instance.values.Max());

            // draw grid
            Handles.DrawLine(new Vector2(area.x, area.y + y), new Vector2(area.xMax, area.y + y));

            if (ylabelStyle == null)
            {
                ylabelStyle = new GUIStyle(EditorStyles.label);
                ylabelStyle.alignment = TextAnchor.UpperRight;
            }

            // add ylabel
            Vector2 vy = new Vector2(area.x - 110f, area.y + y - 10f);

            GUI.Label(new Rect(vy, labelSize), Convert.ToString(instance.values[(ydiv - 1) - yi]), ylabelStyle);
        }

        // plot data
        Handles.color = Color.red;
        if (instance.values.Length > 0)
        {
            var points = new List<Vector3>();
            var dx = area.width / instance.indexs.Max();
            var dy = area.height / instance.values.Max();
            for (var i = 0; i < instance.values.Length; ++i)
            {
                var x = area.x + dx * instance.indexs[i];
                var y = area.yMax - dy * instance.values[i];
                points.Add(new Vector2(x, y));
            }
            Handles.DrawAAPolyLine(2.5f, points.ToArray());
        }
    }
}
#endif