/***
* This code is adapted and modified from
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/ImageWithRoundedCorners.cs
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/Editor/ImageWithIndependentRoundedCornersInspector.cs
**/

using UnityEngine.UI;
using UnityEditor;

namespace TLab.UI.Editor
{
	[CustomEditor(typeof(SDFCircle))]
	public class SDFCircleEditor : UnityEditor.Editor
	{
		private SDFCircle m_instance;

		private void OnEnable()
		{
			m_instance = target as SDFCircle;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			serializedObject.TryDrawProperty("radius", "Radius");
			serializedObject.TryDrawProperty("outlineWidth", "OutlineWidth");
			serializedObject.TryDrawProperty("outlineColor", "OutlineColor");

			serializedObject.ApplyModifiedProperties();

			if (!m_instance.TryGetComponent<MaskableGraphic>(out var _))
			{
				EditorGUILayout.HelpBox("This m_instance requires an MaskableGraphic (Image or RawImage) component on the same gameobject", MessageType.Warning);
			}
		}
	}
}