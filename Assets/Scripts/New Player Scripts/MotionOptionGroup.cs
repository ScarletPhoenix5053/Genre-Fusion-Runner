using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Player/Motion Settings Group")]
public class MotionOptionGroup : ScriptableObject
{
    [Header("Grounded Motion")]
    [Range(0, 1)] public float InstantWalkAcceleration = 0.9f;
    public float WalkSpeed = 5f;
    public float SprintExtraSpeed = 5f;

    [Header("Friction")]
    public float GroundedFriction = 50f;
    public float AirFriction = 10f;
    public float WallrunFriction = 35f;
    public float SlideFriction = 35f;

    [Header("Jumps")]
    public float GroundedJumpStrength = 12f;
    public float GroundedJumpBoost = 2f;
    public float WallJumpStrength = 6f;
    public float WallJumpBoost = 6f;
    public float ClimbJumpForce = 4f;

    [Header("Air Jumps")]
    public int AirJumps = 0;

    [Header("Gravity")]
    public float Gravity = 22f;

    [Header("Aerial Motion")]
    public float AirStrafeForce = 2.2f;

    [Header("Wallrun Limits")]
    public float WallrunSpeed = 12f;
    public float WallrunMinStartSpeed = 5f;
    public float WallrunMinStaySpeed = 0f;

    [Header("Climbing Limits")]
    public float ClimbMaxDuration = 1f;
    public float ClimbSpeed = 5f;

    [Header("Sliding Motion")]
    public float SlideMinSpeed = 1f;
    public float SlideStartForce = 15f;
    public float SlideStartTime = 1f;
    public float SlideStrafeMaxSpeed = 3f;
    public float SlideStrafeStrength = 0.3f;
}