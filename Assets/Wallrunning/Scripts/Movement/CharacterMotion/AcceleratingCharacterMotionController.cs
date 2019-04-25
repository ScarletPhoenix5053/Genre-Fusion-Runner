using UnityEngine;
using System.Collections;

public class AcceleratingCharacterMotionController : BaseMotionController
{
#pragma warning disable 0414
#pragma warning disable 0649
    [SerializeField] private GroundChecker groundChecker;
    [Header("Grounded Motion")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveBackSpeed = 2f;    
    [SerializeField] private float strafeSpeed = 3f;    
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float deceleration = 0.2f;
    [SerializeField] private float acceleration = 0.3f;
    [SerializeField] private float jumpHeight = 10f;
    [Header("Airborne Motion")]
    [SerializeField] private float airStrafeSpeed = 2f;
    [SerializeField] private float gravityAcceleration = 0.2f;
    [SerializeField] private float gravityMax = 10f;

    private bool logHVelThisFrame = false;
#pragma warning restore 0414
#pragma warning restore 0649

    private bool sprint = false;
    private float gravityVel = 0f;
    private Vector2 horizontalVel = Vector2.zero;

    private const float maxLaunchHeight = 50f;
    private const float maxLaunchLateral = 50f;
    private const float maxLaunchFwd = 50f;

    protected bool Grounded => groundChecker != null && groundChecker.Grounded;
    public override float Speed => horizontalVel.y;

    private void OnEnable()
    {
        horizontalVel = Vector2.zero;
    }
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

        // Drag
        if (Mathf.Abs(horizontalVel.x) > deceleration)
        {
            horizontalVel.x -= Mathf.Sign(horizontalVel.x) * deceleration;
        }
        else
        {
            horizontalVel.x = 0;
        }
        if (Mathf.Abs(horizontalVel.y) > deceleration)
        {
            horizontalVel.y -= Mathf.Sign(horizontalVel.y) * deceleration;
        }
        else
        {
            horizontalVel.y = 0;
        }

        // Horizontal motion
        moveVector.x += horizontalVel.x;
        moveVector.z += horizontalVel.y;

        // Apply and reset
        ApplyAndResetMotion();
    }

    public override void MoveHorizontal(Vector2 input)
    {
        if (input == Vector2.zero) return;

        var maxFwdSpeed = moveSpeed * (sprint ? sprintMultiplier : 1);

        if (horizontalVel.y < maxFwdSpeed && input.y > 0)
            horizontalVel.y += (input.y * acceleration) + deceleration;
        if (horizontalVel.y > -moveBackSpeed && input.y < 0)
            horizontalVel.y += (input.y * acceleration) + deceleration;

        if (Mathf.Abs(horizontalVel.x) < strafeSpeed && input.x != 0)
            horizontalVel.x += (input.x * acceleration) + deceleration * Mathf.Sign(input.x);
        
    }
    public override void Jump()
    {
        gravityVel += jumpHeight;

        transform.position += Vector3.up * (groundChecker.CheckRadius + 0.1f);
        translator.AddVelocity(Vector3.up * jumpHeight);
    }
    /// <summary>
    /// Performs a jump with an additional lateral boost
    /// </summary>
    /// <param name="lateralBoost"></param>
    public override void Jump(Vector2 lateralBoost)
    {
        var prevGrav = gravityVel;
        gravityVel = jumpHeight;
        if (gravityVel > maxLaunchHeight) gravityVel = prevGrav;

        var prevVel = horizontalVel;
        horizontalVel += lateralBoost;
        if (horizontalVel.y > maxLaunchFwd) horizontalVel.y = prevVel.y;
        if (Mathf.Abs(horizontalVel.x) > maxLaunchLateral) horizontalVel.x = prevVel.x;
                
        transform.position += Vector3.up * (groundChecker.CheckRadius + 0.1f);
        translator.AddVelocity(Vector3.up * jumpHeight);
    }
    public override void Sprint(bool active)
    {
        sprint = active;
    }
}