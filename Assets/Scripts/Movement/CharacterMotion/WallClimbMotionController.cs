using UnityEngine;
using System.Collections;

public class WallClimbMotionController : BaseMotionController2
{
    [SerializeField] private float jumpOffForce = 5f;
    [SerializeField] private float climbSpeed = 5f;

    public override void MoveHorizontal(Vector2 input)
    {
        cf.AddForce(ToForceOverFixedTime(Vector3.up * climbSpeed * input.y));
    }
}
