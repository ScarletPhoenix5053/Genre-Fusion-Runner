using UnityEngine;
using System;

public class CharacterMotionController : BaseMotionController
{
#pragma warning disable 0649
    [SerializeField] private GroundChecker groundChecker;
    [Header("Grounded Motion")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpHeight = 10f;
    [Header("Airborne Motion")]
    [SerializeField] private float airStrafeSpeed = 2f;
    [SerializeField] private float gravityAcceleration = 0.2f;
    [SerializeField] private float gravityMax = 10f;
#pragma warning restore 0649

    private bool sprint = false;
    private float gravityVel = 0f;

    protected bool Grounded => groundChecker != null && groundChecker.Grounded;
    public override float Speed => moveVector.z;

    private void Awake()
    {
        if (groundChecker == null) Debug.LogWarning("No ground check component assigned: will not use gravity", this);
    }
    private void Update()
    {
        // Gravity
        if (Grounded)
        {
            gravityVel = 0;
        }
        else
        {
            gravityVel = gravityVel > -gravityMax ? gravityVel - gravityAcceleration : -gravityMax;
            moveVector.y = gravityVel;
        }

        // Apply and reset
        ApplyAndResetMotion();
    }

    public override void MoveHorizontal(Vector2 input)
    {
        var motion = new Vector3(input.x, 0, input.y);

        if (Grounded) motion *= moveSpeed * (sprint ? sprintMultiplier : 1);
        else
        {
            motion.z *= moveSpeed * (sprint ? sprintMultiplier : 1);
            motion.x *= airStrafeSpeed;
        }

        moveVector += motion;
    }
    public override void Jump()
    {
        if (!Grounded) return;
        gravityVel += jumpHeight;
        transform.position += Vector3.up * (groundChecker.CheckRadius + 0.1f);
        translator.AddVelocity(Vector3.up * jumpHeight);
    }
    public override void Jump(Vector2 dir)
    {
        throw new System.NotImplementedException();
    }
    public override void Sprint(bool active)
    {
        sprint = active;
    }
}