using UnityEngine;
using System.Collections.Generic;

public class WindEffect : PhysicsEffect
{
    [SerializeField] private float strength = 1f;
    [SerializeField] private Vector3 direction = Vector3.left;

    private List<Rigidbody> allRigidBodies = new List<Rigidbody>();

    private void Start()
    {
        allRigidBodies.AddRange(FindObjectsOfType<Rigidbody>());
    }
    private void FixedUpdate()
    {
        ApplyWind();
    }

    private void ApplyWind()
    {
        foreach (Rigidbody rb in allRigidBodies)
        {
            Debug.Log("Applying wind to " + rb.gameObject.name);
            rb.AddForce(direction * strength * Time.fixedDeltaTime);
        }
    }
}