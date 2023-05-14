using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
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
