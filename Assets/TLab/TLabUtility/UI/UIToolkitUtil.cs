using UnityEngine.UIElements;

namespace TLab.UIElements
{
    public static class VisualElementExtension
    {
        public static T GetElement<T>(this VisualElement rootVE, params string[] elementNameLevel) where T : VisualElement
        {
            VisualElement element = rootVE;

            for (int i = 0; i < elementNameLevel.Length - 1; i++)
            {
                element = element.Q<VisualElement>(elementNameLevel[i]);
            }

            return element.Q<T>(elementNameLevel[elementNameLevel.Length - 1]); ;
        }
    }
}