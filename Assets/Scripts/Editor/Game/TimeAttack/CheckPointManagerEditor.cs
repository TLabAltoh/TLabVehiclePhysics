using UnityEngine;
using UnityEditor;

namespace TLab.Game.TimeAttack.Editor
{
    [CustomEditor(typeof(CheckPointManager))]
    public class CheckPointManagerEditor : UnityEditor.Editor
    {
        private CheckPointManager m_instance;

        private void OnEnable()
        {
            m_instance = target as CheckPointManager;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var width = GUILayout.Width(Screen.width / 3);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("SetUp", width))
            {
                for (int i = 0; i < m_instance.transform.childCount; i++)
                {
                    var child = m_instance.transform.GetChild(i).gameObject;

                    var checkPoint = child.AddComponent<CheckPoint>();

                    var meshRenderer = child.GetComponent<MeshRenderer>();

                    checkPoint.SetUp(m_instance, i, m_instance.target);

                    child.AddComponent<BoxCollider>().isTrigger = true;

                    if (i == m_instance.goal)
                    {
                        checkPoint.SetActive(true);
                        meshRenderer.sharedMaterial = m_instance.matTarget;
                    }
                    else
                    {
                        checkPoint.SetActive(false);
                        meshRenderer.sharedMaterial = m_instance.matSleep;
                    }

                    EditorUtility.SetDirty(child);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
