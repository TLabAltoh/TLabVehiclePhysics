using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TLab.EditorTool
{
    public static class SceneViewCapture
    {
        [MenuItem("Tools/SceneViewCapture")]
        public static void CaptureSceneView()
        {
            string dt = System.DateTime.Now.ToString("yyyy-MM-ddTHHmmss"); // ISO-8601
            string fn = $"{Application.dataPath}/SceneViewScreenshot-{dt}.png";
            CaptureSceneView(fn);
        }

        public static void CaptureSceneView(string pngFilename)
        {
            CaptureSceneView(SceneView.lastActiveSceneView, pngFilename);
        }

        public static void CaptureSceneView(SceneView sceneView, string pngFilename)
        {
            EditorApplication.delayCall += () => {
                var ew = (EditorWindow)sceneView;
                var p = ew.position;
                var w = (int)p.width;
                var h = (int)p.height;
                Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(p.position, w, h);
                var tex2d = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex2d.SetPixels(pixels);
                System.IO.File.WriteAllBytes(pngFilename, tex2d.EncodeToPNG());
                Debug.Log($"CaptureSceneView(pngFilename={pngFilename})");
            };
        }
    }
}
