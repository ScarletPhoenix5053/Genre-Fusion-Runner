using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Player/Motion Settings Group")]
public class MotionOptionGroup : ScriptableObject
{
    [Header("Speed")]
    public float SpeedWalk = 5f;
    public float SpeedRun = 8f;
    public float SpeedWallRun = 12f;
    public float SpeedBoostOnSlide = 2f;
    public float SpeedBoostOnWallRun = 2f;
    public float SpeedBoostOnWallJump = 2f;

    [Header("Gravity")]
    public float GravityStrength = 1f;
    public float GravityMax = 10f;

    [Header("Surface Friction")]
    public float FrictionGround = 5f;
    public float FrictionSlide = 1f;
    public float FrictionWall = 2.5f;

    [Header("In Air")]
    public float AirResistance = 2f;
    public int AddJumps = 0;

    [Header("Jump")]
    public float JumpHeight = 10f;    
}