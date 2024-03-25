/***
* This code is adapted and modified from
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/ImageWithRoundedCorners.cs
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/Editor/ImageWithIndependentRoundedCornersInspector.cs
**/

using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TLab.UI.Editor
{
#if UNITY_EDITOR
	[CustomEditor(typeof(SDFQuad))]
	public class SDFQuadEditor : UnityEditor.Editor
	{
		private SDFQuad m_instance;

		private void OnEnable()
		{
			m_instance = (SDFQuad)target;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			serializedObject.TryDrawProperty("radiusX", "Top Right Corner");
			serializedObject.TryDrawProperty("radiusY", "Bottom Right Corner");
			serializedObject.TryDrawProperty("radiusZ", "Top Left Corner");
			serializedObject.TryDrawProperty("radiusW", "Bottom Left Corner");

			serializedObject.TryDrawProperty("outlineWidth", "OutlineWidth");
			serializedObject.TryDrawProperty("outlineColor", "OutlineColor");

			serializedObject.ApplyModifiedProperties();

			if (!m_instance.TryGetComponent<MaskableGraphic>(out var _))
			{
				EditorGUILayout.HelpBox("This m_instance requires an MaskableGraphic (Image or RawImage) component on the same gameobject", MessageType.Warning);
			}
		}
	}
#endif
}
