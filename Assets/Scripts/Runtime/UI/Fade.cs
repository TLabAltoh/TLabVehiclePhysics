using System.Collections;
using UnityEngine;

namespace TLab
{
    [System.Serializable]
    public class FadeSettings
    {
        public float delay;

        public AnimationCurve curve;

        public float dist0;
        public float dist1;
        public float angle0;
        public float angle1;
    }

    public class Fade : MonoBehaviour
    {
        [SerializeField] private Material m_matFade;

        public static readonly int PROP_DIST = Shader.PropertyToID("_Dist");

        public static readonly int PROP_ANGLE = Shader.PropertyToID("_Angle");

        private IEnumerator FadeTask(FadeSettings settings)
        {
            float current = settings.delay, ratio;

            while ((current -= Time.deltaTime) >= 0)
            {
                ratio = 1f - (current / settings.delay);

                ratio = settings.curve.Evaluate(ratio);

                m_matFade.SetFloat(PROP_DIST, Mathf.Lerp(settings.dist0, settings.dist1, ratio));
                m_matFade.SetFloat(PROP_ANGLE, Mathf.Lerp(settings.angle0, settings.angle1, ratio));

                yield return null;
            }

            ratio = 1.0f;

            m_matFade.SetFloat(PROP_DIST, Mathf.Lerp(settings.dist0, settings.dist1, ratio));
            m_matFade.SetFloat(PROP_ANGLE, Mathf.Lerp(settings.angle0, settings.angle1, ratio));

            yield return null;
        }

        public void Call(FadeSettings settings)
        {
            StartCoroutine(FadeTask(settings));
        }
    }
}
