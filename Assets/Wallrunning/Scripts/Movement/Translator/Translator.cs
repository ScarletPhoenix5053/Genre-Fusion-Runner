using UnityEngine;
using System.Collections;

public class Translator : MonoBehaviour
{
    private Vector3 velocity = Vector3.zero;

    public Vector3 GetVeloctiy() => velocity;
    public void AddVelocity(Vector3 addVel) => velocity += addVel;
    public void SetVelocity(Vector3 newVel) => velocity = newVel;

    protected void Update()
    {
        CommitMovement();
    }

    protected virtual void ProcessMovement()
    {
        throw new System.NotImplementedException();
    }
    protected virtual void CommitMovement()
    {
        Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z);

        vel = transform.TransformDirection(vel);
        transform.position += vel * Time.deltaTime;

        velocity = Vector3.zero;
    }
}