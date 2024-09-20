using UnityEditor;
using UnityEngine;

namespace TLab.LUTTool
{
    public static class LUTEditorUtil
    {
        private static Vector2 LABEL_SIZE = new Vector2(100f, 50f);
        private static GUIStyle m_ylabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };

        public static void CreateXLabel(Rect area, float xOffset, float value, int accuracy)
        {
            var position = new Vector2(area.x + xOffset - 10f, area.yMax);

            GUI.Label(new Rect(position, LABEL_SIZE), value.ToString("0." + new string('0', accuracy)));
        }

        public static void CreateYLable(Rect area, float yOffset, float value, int accuracy)
        {
            var position = new Vector2(area.x - 110f, area.y + yOffset - 10f);

            GUI.Label(new Rect(position, LABEL_SIZE), value.ToString("0." + new string('0', accuracy)), m_ylabelStyle);
        }

        public static Rect DrawPreviewArea()
        {
            var margin = 0.8f;
            var xoffset = Screen.width * (1 - margin) * 0.25f;

            var area = GUILayoutUtility.GetRect(Screen.width * margin, 250f, GUILayout.ExpandWidth(false));
            area.xMax += xoffset;
            area.x += xoffset;

            Handles.DrawSolidRectangleWithOutline(area, Color.black, Color.white);

            return area;
        }

        public static void DrawGrid(Rect area, float div, int xAccuracy, int yAccuracy, float labelXMin, float labelXMax, float labelYMin, float labelYMax)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.5f);

            for (int xi = 1; xi < div - 1; ++xi)
            {
                var lerp = (float)xi / (div - 1);
                var x = lerp * area.width;
                var value = labelXMin * (1f - lerp) + labelXMax * lerp;

                var origin = new Vector2(area.x + x, area.y);

                Handles.DrawLine(origin, origin + new Vector2(0f, area.height));

                CreateXLabel(area, x, value, xAccuracy);
            }

            for (int yi = 1; yi < div - 1; ++yi)
            {
                var lerp = (float)yi / (div - 1);
                var y = area.height * (1f - lerp);
                var value = labelYMin * (1f - lerp) + labelYMax * lerp;

                var origin = new Vector2(area.x, area.y + y);

                Handles.DrawLine(origin + new Vector2(area.width, 0f), origin);

                CreateYLable(area, y, value, yAccuracy);
            }
        }
    }
}
