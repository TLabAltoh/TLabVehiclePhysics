/***
* This code is adapted and modified from
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/ImageWithRoundedCorners.cs
* https://github.com/kirevdokimov/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/Editor/ImageWithIndependentRoundedCornersInspector.cs
**/

using UnityEngine;

namespace TLab.UI
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class SDFCutDisk : SDFUI
	{
		private static readonly string SHAPE_NAME = "CutDisk";

		[SerializeField] public float radius = 40;
		[SerializeField] public float height = 10;

		protected override void OnValidate()
		{
			base.OnValidate();

			Validate(SHAPE_NAME);
			Refresh();
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			Validate(SHAPE_NAME);
			Refresh();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		protected override void Refresh()
		{
			Vector2 halfRect = ((RectTransform)transform).rect.size * .5f;
			m_material.SetVector(PROP_HALFSIZE, halfRect);
			m_material.SetFloat(PROP_RADIUSES, radius);
			m_material.SetFloat(PROP_HEIGHT, height);
			m_material.SetFloat(PROP_OUTLINEWIDTH, outlineWidth);
			m_material.SetColor(PROP_OUTLINECOLOR, outlineColor);
		}
	}
}