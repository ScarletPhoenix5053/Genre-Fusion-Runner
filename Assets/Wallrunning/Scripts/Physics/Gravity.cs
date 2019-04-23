using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private float strength = 1f;
    [SerializeField] private float terminalVelocity = 10f;
    [SerializeField] private Transform transformOverride;
    
    private GroundChecker groundChecker;

    public Vector3 Velocity { get; private set; }

    private void Awake()
    {
        groundChecker = GetComponent<GroundChecker>();
    }

    /// <summary>
    /// Apply accelerating downwards force.
    /// </summary>
    public void Apply()
    {
        Velocity += Vector3.down * strength;
        if (Velocity.y >= terminalVelocity) Velocity = Vector3.down * terminalVelocity;
    }
    /// <summary>
    /// Resets the continuous momentum gain of gravity.
    /// </summary>
    public void Restore()
    {
        Velocity = Vector3.zero;
    }
}
