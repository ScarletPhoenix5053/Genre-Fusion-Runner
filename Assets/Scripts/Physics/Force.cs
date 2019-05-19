using UnityEngine;
using System.Collections;

public static class ToForce
{
    public const float forceMultiplicationFactor = 10000f;

    public static Vector3 Instant(Vector3 force) => force * forceMultiplicationFactor;
    public static Vector3 OverFixedTime(Vector3 force) => force * Time.fixedDeltaTime * forceMultiplicationFactor;
}
