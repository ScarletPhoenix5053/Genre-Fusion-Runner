using UnityEngine;
using System.Collections;

public class GroundedMotionController : BaseMotionController2
{
    protected override void Awake()
    {
        base.Awake();
        refs.GroundChecker.OnGrounding += ResetVelOnGrounding;
    }
    private void FixedUpdate()
    {
        // If grounded, use ground friction
        if (Grounded)
        {
            refs.Drag.DragConstant = friction;
        }
        else
        {
            refs.Drag.DragConstant = airFriction;
        }
        TryGravity();
    }

    #region Jump & Grav
    #region Inspector
#pragma warning disable
    [Header("Airborne Motion")]
    [SerializeField] private float airStrafeForce = 2f;
    [SerializeField] private float gravityForce = 2f;
#pragma warning restore
    #endregion
    private bool groundedLastFrame = false;
    protected bool Grounded => refs.GroundChecker != null && refs.GroundChecker.Grounded;
    private void ResetVelOnGrounding() => refs.CoalescingForce.ResetVelocityY();

    private void TryGravity()
    {
        if (Grounded) return;

        var gravForce = ToForceOverFixedTime(Vector3.down * gravityForce);
        cf.AddForce(gravForce);
    }
    #endregion

    #region Horizontal Motion
    #region Inspector
#pragma warning disable
    [Header("General")]
    [SerializeField] [Range(0, 1)] private float instantAcceleration = 0.5f;
    [SerializeField] private float friction = 50f;
    [SerializeField] private float airFriction = 20f;
    [SerializeField] private float strafeForce = 3f;
    [Header("Walk")]
    [SerializeField] private float walkForce = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.5f;

#pragma warning restore
    #endregion
    public override void MoveHorizontal(Vector2 input)
    {
        base.MoveHorizontal(input);

        CreateForcesByInput();
        ApplyInstantAccelAndDecelAtLowSpeed();
        if (Grounded) AddSprintForce();
        LimitMotionForce(to: walkSpeed);

        ApplyHorizontalMotionForce();
    }
    protected override void CreateForcesByInput()
    {
        MapInputToMotion();
        if (Grounded)
            CreateMotionForce(walkSpeed);
        else
            CreateMotionForce(airStrafeForce);
    }
    private void ApplyInstantAccelAndDecelAtLowSpeed()
    {
        var instantAccelThreshold = walkSpeed * instantAcceleration;
        if (cf.Speed < instantAccelThreshold)
        {
            if (input != Vector2.zero)
            {
                cf.SetVelocity(transform.TransformDirection(motion * instantAccelThreshold));
            }
            else
            {
                cf.ResetVelocityX();
                cf.ResetVelocityZ();
            }
        }
    }
    #endregion

}
