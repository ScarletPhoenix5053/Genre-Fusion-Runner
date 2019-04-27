using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CoalescingForce))]
public class TrueDrag : MonoBehaviour
{
    [SerializeField] [Range(1, 2)] private float velocityExponent = 1f;
    [SerializeField] private float dragConstant;

    private CoalescingForce coalescingForce;

    private void Start()
    {
        coalescingForce = GetComponent<CoalescingForce>();
    }
    private void FixedUpdate()
    {
        var velocity = coalescingForce.Velocity;
        var speed = coalescingForce.Speed;
        var dragStrength = CalculateDrag(speed);
        var drag = -velocity.normalized * dragStrength;

        coalescingForce.AddForce(drag);
    }

    private float CalculateDrag(float velocity)
    {
        return dragConstant * Mathf.Pow(velocity, velocityExponent);
    }
}