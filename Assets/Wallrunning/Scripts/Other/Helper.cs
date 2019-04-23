using UnityEngine;
using System.Collections;

public static class Helper
{
    public static T FindRelevantComponent<T>(Transform obj)
    {
        if (obj.GetComponent<T>() != null) return obj.GetComponent<T>();
        if (obj.GetComponentInParent<T>() != null) return obj.GetComponentInParent<T>();
        if (obj.GetComponentInChildren<T>() != null) return obj.GetComponentInChildren<T>();

        // Return default if nothing found
        var type = typeof(T);
        Debug.LogWarning("Can't find a component or type " + type +" on self, parent or child.", obj);
        return default;
    }
}
