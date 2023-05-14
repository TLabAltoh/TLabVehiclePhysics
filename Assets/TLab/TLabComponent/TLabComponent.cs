using System.Collections.Generic;
using UnityEngine;

public static class TLabComponent
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

    public static GameObject[] FindGameObjectsWithLayerMask(LayerMask layerMask)
    {
        GameObject[] goArray = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> goList = new List<GameObject>();
        foreach (GameObject go in goArray)
        {
            // LayerMask bit check
            if (((1 << go.layer) & layerMask.value) != 0)
            {
                goList.Add(go);
            }
        }
        if (goList.Count == 0)
        {
            return null;
        }
        return goList.ToArray();
    }
}
