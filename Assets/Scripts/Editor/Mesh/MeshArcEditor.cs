using UnityEngine;
using UnityEditor;

namespace TLab.MeshEngine.Editor
{
    [CustomEditor(typeof(MeshArc))]
    public class MeshArcEditor : UnityEditor.Editor
    {
        private MeshArc m_instance;

        private void OnEnable()
        {
            m_instance = target as MeshArc;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var width = GUILayout.Width(Screen.width / 3);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Instantiate", width))
            {
                m_instance.Instantiate(
                    m_instance.transform.position,
                    m_instance.transform.rotation, false, nameof(MeshArc));
            }

            if (GUILayout.Button("Cash", width))
            {
                m_instance.Cash();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
