using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simulate and exert physical forces on the attatched object.
/// </summary>
public class CoalescingForce : Translator
{/*
    private List<Vector3> forces = new List<Vector3>();
    private Vector3 sumOfForces = Vector3.zero;

    public override void AddVelocity(Vector3 addVel)
    {
        forces.Add(addVel);
    }

    protected override void Update()
    {
        ProcessForces();
        CommitMovement();
    }

    protected override void ProcessForces()
    {
        // Sum forces
        foreach (Vector3 force in forces)
        {
            sumOfForces += force;
        }

        velocity += sumOfForces;
    }
    protected override void CommitMovement()
    {
        Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z);

        vel = transform.TransformDirection(vel);
        transform.position += vel * Time.deltaTime;
    }
    */
}
