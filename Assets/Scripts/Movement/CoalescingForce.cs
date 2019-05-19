using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simulate and exert physical forces on the attatched object.
/// </summary>
public class CoalescingForce : MonoBehaviour
{
    #region Runtime
    private void Awake()
    {
        InitForceRenderer();
    }


    private void FixedUpdate()
    {
        UpdateNetForce();
        if (netForce != Vector3.zero)
        {
            ApplyAcceleration();
        }

        ApplyMotion();
        forceRenderer.UpdateForceData();
        ResetForceList();
    }
    private void OnDrawGizmos()
    {
        if (renderForces && forceRenderer != null)
        {
            forceRenderer.DrawForceGizmos();
        }
    }
    #endregion

    #region Force Rendering
#pragma warning disable
    private CoalescingForceRenderer forceRenderer;
    private bool renderForces = true;
#pragma warning restore

    private void InitForceRenderer()
    {
        forceRenderer = new CoalescingForceRenderer(objectToWatch: this);
    }
    #endregion

    #region Mass
    [SerializeField] private float mass = 1f;                   // [kg]

    public float Mass { get => mass; set { mass = value; } }
    #endregion
    #region Velocity
    [SerializeField] private Vector3 velocity = Vector3.zero;   // [m s^-1]
    public Vector3 Velocity => velocity;

    public float ForwardVel => transform.InverseTransformDirection(velocity).z;
    public float Speed => velocity.magnitude;

    public void ResetVelocityX() => velocity.x = 0;
    public void ResetVelocityY() => velocity.y = 0;
    public void ResetVelocityZ() => velocity.z = 0;
    public void SetVelocity(Vector3 newVel) => velocity = newVel;

    private void ApplyMotion()
    {
        // Get change in position over fixed update
        var deltaS = velocity * Time.fixedDeltaTime;
        transform.position += deltaS;
    }
    #endregion
    #region Force
    [SerializeField] private Vector3 netForce = Vector3.zero;   //  N [kg m s^-2]
    public Vector3 NetForce => netForce;

    public List<Vector3> Forces { get; private set; } = new List<Vector3>();

    public void AddForce(Vector3 force)
    {
        Forces.Add(force);
    }
    public IEnumerator AddForceOverTime(Vector3 force, float t)
    {
        if (forceOverTimeRoutine != null) StopCoroutine(forceOverTimeRoutine);
        forceOverTimeRoutine = ForceOverTimeRoutine(force, t);
        StartCoroutine(forceOverTimeRoutine);
        return forceOverTimeRoutine;
    }
    public void CancelForceOverTime(IEnumerator routine)
    {
        if (routine == null) return;
        StopCoroutine(routine);
    }
    private IEnumerator forceOverTimeRoutine;
    private IEnumerator ForceOverTimeRoutine(Vector3 force, float t)
    {        
        while (t > 0)
        {
            AddForce(force);

            t -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
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

    private void ApplyAcceleration()
    {
        var acceleration = Vector3.zero;
        acceleration = netForce / mass;

        velocity += acceleration * Time.fixedDeltaTime;
    }
    #endregion
}
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