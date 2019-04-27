using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerRefs2))]
[RequireComponent(typeof(CoalescingForce))]
public class GroundedMotionController : BaseMotionController
{
    private PlayerRefs2 refs;
    private CoalescingForce cf;
    private const float forceMultiplicationFactor = 10000f;
    private Vector2 input;

    private void Awake()
    {
        refs = GetComponent<PlayerRefs2>();
        cf = refs.CoalescingForce;

        refs.GroundChecker.OnGrounding += ResetVelOnGrounding;
    }
    private void FixedUpdate()
    {
        TryGravity();
    }

    #region Jump & Grav
    [Header("Airborne Motion")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float airStrafeForce = 2f;
    [SerializeField] private float gravityForce = 2f;

    private bool groundedLastFrame = false;
    protected bool Grounded => refs.GroundChecker != null && refs.GroundChecker.Grounded;
    private void ResetVelOnGrounding() => refs.CoalescingForce.ResetVelocityY();

    public override void Jump()
    {
        refs.CoalescingForce.AddForce(Vector3.up * jumpForce * forceMultiplicationFactor);
    }
    public override void Jump(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }

    private void TryGravity()
    {
        if (Grounded) return;

        var gravForce = Vector3.down * gravityForce * (Time.fixedDeltaTime * forceMultiplicationFactor);
        refs.CoalescingForce.AddForce(gravForce);
    }
    #endregion

    #region Horiontal Motion
    [Header("Grounded Motion")]
    [SerializeField] private float walkForce = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] [Range(0,1)] private float instantAcceleration = 0.5f;
    [SerializeField] private float walkBackForce = 2f;
    [SerializeField] private float strafeForce = 3f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    private Vector3 motion = Vector3.zero;
    private Vector3 motionForce = Vector3.zero;

    public override float Speed => throw new System.NotImplementedException();

    public override void MoveHorizontal(Vector2 input)
    {
        this.input = input;

        CreateForcesByInput();
        ApplyInstantAccelAndDecelAtLowSpeed();
        ApplySpeedLimitsToForces();

        ApplyHorizontalMotionForce();
    }

    private void CreateForcesByInput()
    {
        motion = new Vector3(this.input.x, 0, this.input.y);
        motionForce = motion * walkForce * Time.fixedDeltaTime * forceMultiplicationFactor;
        motionForce = transform.TransformDirection(motionForce);
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
    private void ApplySpeedLimitsToForces()
    {
        motionForce = Vector3.ClampMagnitude(motionForce, walkSpeed * forceMultiplicationFactor);
    }
    private void ApplyHorizontalMotionForce()
    {
        cf.AddForce(motionForce);
    }


    public override void Sprint(bool active)
    {
        throw new System.NotImplementedException();
    }
    #endregion

}
