using UnityEngine;

namespace TLab
{
    public static class ComponentUtility
    {
        public static T GetComponentFromParent<T>(Transform transform, string typeName)
        {
            Transform parent = transform;

            while (parent != null)
            {
                // Cast so that the comparison operator (==) of UnityEngine.Object can be used
                UnityEngine.Object obj = parent.GetComponent<T>() as UnityEngine.Object;

                if (obj != null)
                {
                    return parent.GetComponent<T>();
                }

                parent = parent.parent;
            }

            Debug.LogError(typeName + " : Unable to find associated " + typeof(T).Name);

            return default(T);
        }
    }
}
