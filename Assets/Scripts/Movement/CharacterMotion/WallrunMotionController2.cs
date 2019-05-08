using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunMotionController2 : BaseMotionController2
{
    #region Forward Motion
    [Header("Wallrun Speed")]
    [SerializeField] private float wallrunStartSpeed = 12f;
    [Header("Friction")]
    [SerializeField] private float friction = 35f;

    public override void MoveHorizontal(Vector2 input)
    {
        base.MoveHorizontal(input);
        refs.Drag.DragConstant = friction;

        CreateForcesByInput();
        LimitMotionForce(to: wallrunStartSpeed);

        ApplyHorizontalMotionForce();
    }

    protected override void CreateForcesByInput()
    {
        MapInputToMotion();
        CreateMotionForce(wallrunStartSpeed);
    }
    protected override void MapInputToMotion()
    {
        motion = new Vector3(0, 0, input.y);
    }
    #endregion

    #region Jump/Gravity

    #endregion
}