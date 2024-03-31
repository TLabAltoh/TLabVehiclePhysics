using UnityEngine;
using UnityEngine.UI;

namespace TLab.UI
{
	public class SDFUI : MonoBehaviour
	{
		public static readonly int PROP_HALFSIZE = Shader.PropertyToID("_halfSize");
		public static readonly int PROP_RADIUSES = Shader.PropertyToID("_radius");
		public static readonly int PROP_THETA = Shader.PropertyToID("_theta");
		public static readonly int PROP_WIDTH = Shader.PropertyToID("_width");
		public static readonly int PROP_HEIGHT = Shader.PropertyToID("_height");
		public static readonly int PROP_OUTLINECOLOR = Shader.PropertyToID("_outlineColor");
		public static readonly int PROP_OUTLINEWIDTH = Shader.PropertyToID("_outlineWidth");

		[SerializeField] public float outlineWidth = 10;
		[SerializeField] public Color outlineColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);

		protected Material m_material;

		public Material material => m_material;

		[HideInInspector, SerializeField] protected MaskableGraphic m_image;

		protected virtual void Validate(string shape)
		{
			if (m_material == null)
			{
				m_material = new Material(Shader.Find("UI/SDF/" + shape));
			}

			if (m_image == null)
			{
				TryGetComponent(out m_image);
			}

			if (m_image != null)
			{
				m_image.material = m_material;
			}
		}

		protected virtual void OnRectTransformDimensionsChange()
		{
			if (enabled && m_material != null)
			{
				Refresh();
			}
		}

		protected virtual void OnValidate()
		{
		}

		protected virtual void OnEnable()
		{
			var other2 = GetComponent<SDFUI>();
			if (other2 != null && other2 != this)
			{
				DestroyHelper.Destroy(other2);
			}
		}

		protected virtual void OnDestroy()
		{
			m_image.material = null;    // This makes so that when the component is removed, the UI m_material returns to null

			DestroyHelper.Destroy(m_material);
			m_image = null;
			m_material = null;
		}

		protected virtual void Refresh()
		{
		}
	}
}
