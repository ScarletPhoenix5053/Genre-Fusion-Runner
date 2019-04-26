using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneCharMotionController : BaseMotionController
{
    [SerializeField] private float airStrafeSpeed = 2f;
    public override float Speed => moveVector.z;

    public override void Jump()
    {
        throw new System.NotImplementedException();
    }
    public override void Jump(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }
    public override void MoveHorizontal(Vector2 input)
    {
        throw new System.NotImplementedException();
    }
    public override void Sprint(bool active)
    {
        throw new System.NotImplementedException();
    }
}
