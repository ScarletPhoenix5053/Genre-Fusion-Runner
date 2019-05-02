using UnityEngine;
using System.Collections;

public class SlideMotionController2 : BaseMotionController2
{
    private void OnEnable()
    {
        ApplyInitialSlideForce();
    }

    #region Slide General
    #region Inspector
#pragma warning disable
    [Header("Slide")]
    [SerializeField] private float slideStartForce;
    [SerializeField] private float maxSlideSpeed = 12f;
    [SerializeField] private float friction = 0.2f;
#pragma warning restore
    #endregion
    public float SlideFriction { get => friction; set { friction = value; } }
    private void ApplyInitialSlideForce()
    {
        var startForce = -(transform.forward * slideStartForce * forceMultiplicationFactor);
        cf.AddForce(-startForce);
    }
    #endregion

    #region Strafe
    #region Inspector
#pragma warning disable
    [Header("Strafe")]
    [SerializeField] private float strafeMaxSpeed = 3f;
    [SerializeField] private float strafeStrength = 0.3f;
#pragma warning restore
    #endregion
    public override void MoveHorizontal(Vector2 input)
    {
        base.MoveHorizontal(input);

        CreateForcesByInput();
        LimitMotionForce(to: strafeMaxSpeed);

        ApplyHorizontalMotionForce();
    }
    protected override void CreateForcesByInput()
    {
        MapInputToMotion();
        CreateMotionForce(strafeStrength);
    }
    protected override void MapInputToMotion()
    {
        // Only map x motion, as player can only apply strafe force during slide
        motion = new Vector3(this.input.x, 0, 0);
    }
    #endregion
}