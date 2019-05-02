using UnityEngine;
using System.Collections;
using SCARLET.DbOverlay;

[RequireComponent(typeof(Rigidbody))]
public class RigidBodyLogger : MonoBehaviour
{
    [SerializeField] private Vector3 velocity;
    [SerializeField] private Vector3 velocityLocal;

    private Rigidbody rb;

    private const string idVelWorld = "World Velocity";
    private const string idVelLocal = "Local Velocity";

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        DebugOverlay.CreateLog(SignWithName(idVelWorld));
        DebugOverlay.CreateLog(SignWithName(idVelLocal));
    }
    private void FixedUpdate()
    {
        velocity = rb.velocity;
        velocityLocal = transform.InverseTransformDirection(velocity);

        DebugOverlay.UpdateLog(SignWithName(idVelWorld), velocity.ToString());
        DebugOverlay.UpdateLog(SignWithName(idVelLocal), velocityLocal.ToString());
    }

    private string SignWithName(string label) => name + " | " + label;
}
