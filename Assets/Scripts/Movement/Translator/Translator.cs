using UnityEngine;
using System.Collections.Generic;

public class Translator : MonoBehaviour
{
    protected virtual void Update()
    {
        CommitMovement();
    }
    protected virtual void CommitMovement()
    {
        Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z);
        //vel += totalForce;

        vel = transform.TransformDirection(vel);
        transform.position += vel * Time.deltaTime;

        velocity = Vector3.zero;
    }

    #region Velocity
    protected Vector3 velocity = Vector3.zero;
    public virtual Vector3 GetVeloctiy() => velocity;
    public virtual void AddVelocity(Vector3 addVel) => velocity += addVel;
    public virtual void SetVelocity(Vector3 newVel) => velocity = newVel;
    #endregion
    #region Forces
    /*
    protected virtual void ProcessForces()
    {
        foreach (Vector3 force in activeForces)
        {
            totalForce += force;
        }
    }
    private List<Vector3> activeForces = new List<Vector3>();
    private Vector3 totalForce = Vector3.zero;
    */
    #endregion

}