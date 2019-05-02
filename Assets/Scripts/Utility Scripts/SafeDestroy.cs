/* 
 * ===========================================================================
 * Solution provided by VesuvianPrime
 * Retrieved from https://answers.unity.com/questions/894959/addingremoving-objects-in-editor-mode.html 
 * =========================================================================== 
 */

using UnityEngine;

/// <summary>
/// Collection of static methods for safley destroying game objects in unity
/// </summary>
public class SafeDestroy
{
    /// <summary>
    /// Safely destroys any unity object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T Object<T>(T obj) where T : Object
    {
        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(obj);
        else
            UnityEngine.Object.Destroy(obj);

        return null;
    }
    /// <summary>
    /// Safely estroys the game object the passed component is attatched to.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <returns></returns>
    public static T GameObject<T>(T component) where T : Component
    {
        if (component != null)
            Object(component.gameObject);
        return null;
    }
}