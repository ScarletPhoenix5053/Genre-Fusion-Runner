using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simulate and exert physical forces on the attatched object.
/// </summary>
public class CoalescingForce : MonoBehaviour
{
#pragma warning disable 0618
    [SerializeField] private float mass = 1f;                   // [kg]
    [SerializeField] private Vector3 velocity = Vector3.zero;   // [m s^-1]
    [SerializeField] private Vector3 netForce = Vector3.zero;   //  N [kg m s^-2]
#pragma warning restore 0618
    public float Mass { get => mass; set { mass = value; } }
    public Vector3 Velocity => velocity;
    public Vector3 NetForce => netForce;

    public float Speed => velocity.magnitude;

    public List<Vector3> Forces { get; private set; } = new List<Vector3>();

    public event PhysicsUpdateHandler OnPhysicsUpdate;

    private CoalescingForceRenderer forceRenderer;
    private bool renderForces = false;

    #region Unity Runtime
    private void Awake()
    {
        // Inject dependencies
        forceRenderer = new CoalescingForceRenderer(objectToWatch: this);
        renderForces = true;

        // Show/Hide trails
    }
    private void FixedUpdate()
    {
        UpdateNetForce();
        if (netForce != Vector3.zero)
        {
            ApplyAcceleration();
        }

        ApplyMotion();
        OnPhysicsUpdate?.Invoke();
        forceRenderer.UpdateForceData();
        ResetForceList();
    }
    private void OnDrawGizmos()
    {
        if (renderForces)
        {
            forceRenderer.DrawForceGizmos();
        }
    }
    #endregion

    public void ResetVelocityX() => velocity.x = 0;
    public void ResetVelocityY() => velocity.y = 0;
    public void ResetVelocityZ() => velocity.z = 0;
    public void SetVelocity(Vector3 newVel) => velocity = newVel;
    /// <summary>
    /// Add a new force to this object
    /// </summary>
    /// <param name="force"></param>
    public void AddForce(Vector3 force)
    {
        Forces.Add(force);
    }

    /// <summary>
    /// Calculate the combined value of all forces applied to this object.
    /// </summary>
    /// <remarks>
    /// Logs a warning if total force is a net force (total force of 0).
    /// </remarks>
    /// <returns></returns>
    private void UpdateNetForce()
    {
        netForce = Vector3.zero;

        // Step thru forces and add them
        for (int i = 0; i < Forces.Count; i++)
        {
            var force = Forces[i];
            netForce += force;
        }
    }
    private void ResetForceList()
    {
        Forces = new List<Vector3>();
    }

    /// <summary>
    /// Create acceleration based on the mass and forces acting upon this object
    /// </summary>
    private void ApplyAcceleration()
    {
        var acceleration = Vector3.zero;
        acceleration = netForce / mass;

        velocity += acceleration * Time.fixedDeltaTime;
    }
    /// <summary>
    /// Moves the object based on velocity
    /// </summary>
    private void ApplyMotion()
    {
        // Get change in position over fixed update
        var deltaS = velocity * Time.fixedDeltaTime;
        transform.position += deltaS;
    }
}
public delegate void PhysicsUpdateHandler();
public class CoalescingForceRenderer
{
    private const float forceMultiplicationFactor = 1000f;
    private CoalescingForce coalescingForce;
    private Transform transform;

    private readonly Color indvForceColour = Color.cyan;
    private readonly Color netForceColour = Color.magenta;

    private List<Vector3> allForces;

    public CoalescingForceRenderer(CoalescingForce objectToWatch)
    {
        coalescingForce = objectToWatch;
        transform = coalescingForce.transform;
        allForces = coalescingForce.Forces;
    }

    /// <summary>
    /// Must be called from inside OnDrawGizmos
    /// </summary>
    public void DrawForceGizmos()
    {
        DrawLinesForEachForce();
        DrawLineForNetForce();
    }
    public void UpdateForceData()
    {
        allForces = coalescingForce.Forces;
    }

    private void DrawLineForNetForce()
    {
        Gizmos.color = netForceColour;
        Gizmos.DrawLine(transform.position, transform.position - coalescingForce.NetForce);
    }

    private void DrawLinesForEachForce()
    {
        Gizmos.color = indvForceColour;
        foreach (Vector3 force in allForces)
        {
            Gizmos.DrawLine(transform.position, transform.position - (force / forceMultiplicationFactor));
        }
        //Debug.Log(allForces.Count);
        //Debug.Log(allForces[0]);
        //Debug.Log(allForces[1]);
    }
}