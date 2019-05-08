using UnityEngine;
using System.Collections;

public class SlideMotionController : BaseMotionController
{
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private float friction = 35f;
    [SerializeField] private float strafeStrength = 0.3f;
    [SerializeField] private float strafeMaxSpeed = 3f;

    private Vector2 horizontalVel = Vector2.zero;

    public float SlideFriction { get => friction; set { friction = value; } }
    public override float Speed => horizontalVel.y;

    private void OnEnable()
    {
        horizontalVel = Vector2.zero;
        horizontalVel.y = slideSpeed;
    }
    private void Update()
    {
        // Drag
        if (Mathf.Abs(horizontalVel.x) > friction)
        {
            horizontalVel.x -= Mathf.Sign(horizontalVel.x) * friction;
        }
        else
        {
            horizontalVel.x = 0;
        }
        if (Mathf.Abs(horizontalVel.y) > friction)
        {
            horizontalVel.y -= Mathf.Sign(horizontalVel.y) * friction;
        }
        else
        {
            horizontalVel.y = 0;
        }

        // Horizontal motion
        moveVector.x += horizontalVel.x;
        moveVector.z += horizontalVel.y;

        // Apply and reset
        ApplyAndResetMotion();
    }

    public override void Jump()
    {
        // Launch player into air?? or handle thru main motion controller??
        throw new System.NotImplementedException();
    }
    public override void Jump(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }
    public override void MoveHorizontal(Vector2 input)
    {
        // Pull towards left and right based on x input
        if (Mathf.Abs(horizontalVel.x) < strafeMaxSpeed && input.x != 0)
            horizontalVel.x += (input.x * strafeStrength) + friction * Mathf.Sign(input.x);
    }
    public override void Sprint(bool active)
    {
        // Do nothing
        //throw new System.NotImplementedException();
    }
}
