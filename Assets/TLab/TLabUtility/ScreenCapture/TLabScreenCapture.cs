using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TLab
{
#if UNITY_EDITOR
    public class TLabScreenCapture : MonoBehaviour
    {
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] string savePath;

        public void Capture()
        {
            var renderCamera = GetComponent<Camera>();
            if (renderCamera == null)
            {
                Debug.LogError("RenderingCamera was not found");

                return;
            }

            var size = new Vector2Int(width, height);
            var render = new RenderTexture(size.x, size.y, 24);
            var texture = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

            try
            {
                renderCamera.targetTexture = render;
                renderCamera.Render();

                RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                texture.Apply();
            }
            finally
            {
                renderCamera.targetTexture = null;
                RenderTexture.active = null;
            }

            File.WriteAllBytes($"{Application.dataPath}" + $"/{savePath}", texture.EncodeToPNG());

            AssetDatabase.Refresh();
        }
    }

    [CustomEditor(typeof(TLabScreenCapture))]
    public class TLabScreenCaptureEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Capture"))
            {
                TLabScreenCapture capture = target as TLabScreenCapture;
                capture.Capture();
            }
        }
    }
#endif
}
