using UnityEngine;
using System.Collections;

public class SlideMotionController2 : BaseMotionController2
{
    private void OnEnable()
    {
        ApplyInitialSlideForce();
        refs.Drag.DragConstant = friction;
    }
    private void OnDisable()
    {
        cf.CancelForceOverTime(forceOverTimeRoutine);
    }

    #region Slide General 
    #region Inspector
#pragma warning disable
    [Header("Slide")]
    [SerializeField] private float slideStartForce;
    [SerializeField] private float slideStartTime = 1f;
    [SerializeField] private float maxSlideSpeed = 12f;
    [SerializeField] private float friction = 35f;
    private IEnumerator forceOverTimeRoutine;
#pragma warning restore
    #endregion
    public float SlideFriction { get => friction; set { friction = value; } }
    private void ApplyInitialSlideForce()
    {
        var startForce = -(ToForceOverFixedTime(transform.forward * slideStartForce));
        forceOverTimeRoutine = cf.AddForceOverTime(-startForce, slideStartTime);
    }
    #endregion

    #region Strafe
    #region Inspector
#pragma warning disable
    [Header("Strafe")]
    [SerializeField] private float slideStrafeMaxSpeed = 3f;
    [SerializeField] private float slideStrafeStrength = 0.3f;
#pragma warning restore
    #endregion
    public override void MoveHorizontal(Vector2 input)
    {
        base.MoveHorizontal(input);

        CreateForcesByInput();
        LimitMotionForce(to: slideStrafeMaxSpeed);

        ApplyHorizontalMotionForce();
    }
    protected override void CreateForcesByInput()
    {
        MapInputToMotion();
        CreateMotionForce(slideStrafeStrength);
    }
    protected override void MapInputToMotion()
    {
        // Only map x motion, as player can only apply strafe force during slide
        motion = new Vector3(this.input.x, 0, 0);
    }
    #endregion
}