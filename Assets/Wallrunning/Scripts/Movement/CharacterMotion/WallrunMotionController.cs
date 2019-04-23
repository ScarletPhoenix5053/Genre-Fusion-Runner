using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunMotionController : BaseMotionController
{
    [SerializeField] private WallRunDetector detector;
    [SerializeField] private float wallrunMaxSpeed = 12f;
    [SerializeField] private float acceleration = 0.3f;
    [SerializeField] private float deceleration = 0.2f;

    private float momentum = 0f;
    
    public override float Speed => momentum;

    private void Awake()
    {
        if (detector == null) detector = Helper.FindRelevantComponent<WallRunDetector>(transform);
        detector.OnNewWallDetected += NewWallRun;
    }
    private void Update()
    {
        // Apply drag
        if (Mathf.Abs(momentum) > deceleration)
        {
             momentum -= Mathf.Sign(momentum) * deceleration;
        }
        else
        {
             momentum = 0;
        }

        // Apply momentum to moveVector
        moveVector.z += momentum;
        ApplyAndResetMotion();
    }

    public override void MoveHorizontal(Vector2 input)
    {
        // Increase momentum
        if (momentum < wallrunMaxSpeed)
            momentum += input.y * (acceleration);
    }
    public override void Jump()
    {

    }
    public override void Jump(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }
    public override void Sprint(bool active)
    {

    }

    private void NewWallRun(BoxCollider newWallTransform, int newWallDirection)
    {
        momentum = wallrunMaxSpeed;
    }
}