using UnityEngine;
using System.Collections;

public static class MyExtensions
{
    public static Vector3 Flatten(this Vector2 v2) => new Vector3(v2.x, 0, v2.y);
}
