using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Player/Motion Settings Group")]
public class MotionOptionGroup : ScriptableObject
{
    [Header("Speed")]
    public float SpeedWalk = 5f;
    public float SpeedRunMultiplier = 1.5f;
    public float SpeedWallRun = 12f;
    public float SpeedBoostOnSlide = 2f;
    public float SpeedBoostOnWallRun = 2f;
    public float SpeedBoostOnWallJump = 2f;

    [Header("Gravity")]
    public float GravityStrength = 1f;
    public float GravityMax = 10f;
    public GroundCheckVars GoundCheck;
    [System.Serializable]
    public class GroundCheckVars
    {
        [Header("General")]
        public float actorHeight = 2f;
        public LayerMask surfaceMask;
        [Header("Smoothing")]
        public bool smooth;
        public float smoothSpeed;
        [Header("Casting")]
        public Vector3 CastOrigin = new Vector3(0, 1.2f, 0);
        public Vector3 CheckPoint = new Vector3(0, -0.87f, 0);
        public float surfaceSphereCastRadius = 0.17f;
        public float surfaceSphereCastDist = 20f;
        public float CheckRadius = 0.57f;
    }

    [Header("Surface Friction")]
    public float FrictionGround = 5f;
    public float FrictionSlide = 1f;
    public float FrictionWall = 2.5f;

    [Header("In Air")]
    public float AirStrafe = 1f;
    public float AirResistance = 2f;
    public int AddJumps = 0;

    [Header("Jump")]
    public float JumpHeight = 10f;    
}