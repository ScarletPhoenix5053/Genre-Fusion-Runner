using System.Collections;
using System.Collections.Generic;
using SCARLET.DbOverlay;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Pref Files")]
    [SerializeField] private PlayerPreferenceGroup prefs;
    [SerializeField] private MotionOptionGroup motion;

    private IPlayerInput inputGroup;
    private PlayerState state;

    [SerializeField] protected float jumpForce = 12f;
    [SerializeField] private float gravityForce = 2f;
    private CoalescingForce cf;
    private TrueDrag drag;
    private const float forceMultiplicationFactor = 100f;
    private Vector3 ToForceInstant(Vector3 force) => force * forceMultiplicationFactor;
    private Vector3 ToForceOverFixedTime(Vector3 force) => force * Time.fixedDeltaTime * forceMultiplicationFactor;

    #region Grounding
    private bool grounded = false;
    private bool groundedLastFrame = true;
    private MotionOptionGroup.GroundCheckVars gc;
    private RaycastHit groundHit;
    
    public float CheckRadius => gc.CheckRadius;
    protected Vector3 CastDirection
    {
        get
        {
            var a = transform.TransformPoint(gc.CastOrigin);
            var b = transform.TransformPoint(gc.CheckPoint);
            return b - a;
        }
    }

    public event GroundingEventHandler OnGrounding;

    public bool Grounded
    {
        get
        {
            CheckGrounding();
            return grounded;
        }
    }
    private void CheckGrounding()
    {
        // Init vars
        grounded = false;
        var ray =
            new Ray(
                transform.TransformPoint(gc.CastOrigin),
                CastDirection);
        RaycastHit tempHit = new RaycastHit();

        // Cast initial ray
        if (Physics.SphereCast(ray, gc.surfaceSphereCastRadius, out tempHit, gc.surfaceSphereCastDist, gc.surfaceMask, QueryTriggerInteraction.Ignore))
        {
            // Confirm hit ground
            ConfirmGrounding(tempHit);
        }

        if (groundedLastFrame && !grounded) groundedLastFrame = false;
    }
    private void ConfirmGrounding(RaycastHit tempHit)
    {
        // Check area around grounding point
        Collider[] colBuffer = new Collider[3];
        int num =
            Physics.OverlapSphereNonAlloc(
                transform.TransformPoint(gc.CheckPoint),
                gc.CheckRadius,
                colBuffer,
                gc.surfaceMask,
                QueryTriggerInteraction.Ignore
                );

        // validate grounding
        for (int i = 0; i < num; i++)
        {
            // If valid
            if (colBuffer[i].transform == tempHit.transform)
            {
                groundHit = tempHit;
                // Invoke onGrounding if not on ground last frame
                if (!groundedLastFrame)
                {
                    OnGrounding?.Invoke();
                    groundedLastFrame = true;
                }
                grounded = true;

                // Snap to surface
                if (!gc.smooth)
                {
                    transform.position =
                        new Vector3(
                            transform.position.x,
                            groundHit.point.y + gc.actorHeight / 2,
                            transform.position.z
                            );
                }
                else
                {
                    transform.position = Vector3.Lerp(
                        transform.position,
                        new Vector3(
                            transform.position.x,
                            groundHit.point.y + gc.actorHeight / 2,
                            transform.position.z
                            ),
                        gc.smoothSpeed * Time.deltaTime
                        );
                }
                break;
            }
        }
    }
    #endregion

    #region Debug
    private const string idState = "Player State";
    #endregion

    #region Cam
    private void UpdateCam(float lag = 0f)
    {
        var look = inputGroup.GetAxisLook();
        FindObjectOfType<CameraController>().Turn(look, lag);
    }
    #endregion

    #region Motion
    private Vector3 motionForce = Vector3.zero;

    private void ApplyForce(Vector3 force) => cf.AddForce(force);
    private void MoveByInput(Vector2 motionAxis, float speed)
    {
        // Move player by input
        var motionDir = new Vector3(motionAxis.x, 0, motionAxis.y).normalized;
        motionForce = ToForceOverFixedTime(motionDir * speed);
        motionForce = transform.TransformDirection(motionForce);
    }
    private void JumpIf(bool input)
    {
        // Try jump
        if (input)
        {
            state = PlayerState.Jumping;
            transform.position += Vector3.up * gc.CheckRadius;
            cf.AddForce(ToForceInstant(Vector3.up * motion.JumpHeight));
        }
    }
    #endregion

    #region Runtime
    private void Awake()
    {
        inputGroup = new KBMInputGroup(prefs);
        gc = motion.GoundCheck;

        // temp
        cf = GetComponent<CoalescingForce>();
        drag = GetComponent<TrueDrag>();

        // Init logs
        DebugOverlay.CreateLog(idState);
    }
    private void Update()
    {
        // Cam
        UpdateCam();

        // Vars for cycle
        var jump = inputGroup.GetInputJump();
        var sprint = inputGroup.GetInputSprint();
        var motionAxis = inputGroup.GetAxisMotion();

        // Do action by input
        switch (state)
        {
            #region Still
            case PlayerState.Still:
                // Ensure player is grounded
                if (!Grounded)
                {
                    state = PlayerState.Jumping;
                    break;
                }
                else
                {
                    JumpIf(input: jump);

                    // Try move
                    if (motionAxis != Vector2.zero)
                    {
                        state = PlayerState.GroundedMotion;
                    }
                }
                break;
            #endregion
            #region Grounded Motion
            case PlayerState.GroundedMotion:
                // Stop moving when momentum is low
                if (motionAxis == Vector2.zero)
                {
                    state = PlayerState.Still;
                }
                else
                {
                    MoveByInput(motionAxis, motion.SpeedWalk);
                    JumpIf(input: jump);

                    // Try sprint
                    if (sprint)
                    {
                        motionForce *= motion.SpeedRunMultiplier;
                    }

                    ApplyForce(motionForce);
                }
                break;
            #endregion
            #region Jump
            case PlayerState.Jumping:
                // Land on grounding
                if (Grounded)
                {
                    state = PlayerState.Still;
                    cf.ResetVelocityY();
                    break;
                }
                else
                {
                    MoveByInput(motionAxis, motion.AirStrafe);
                    ApplyForce(motionForce);

                    // Apply grav
                    var gravForce = ToForceOverFixedTime(Vector3.down * motion.GravityStrength);
                    cf.AddForce(gravForce);
                }
                break;
            #endregion

            default: throw new System.NotImplementedException("State interactions for state " 
                + state + " are not implimented");
        }

        // Update logs
        DebugOverlay.UpdateLog(idState, state.ToString());
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(gc.CheckPoint), gc.surfaceSphereCastRadius);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.TransformPoint(gc.CastOrigin), CastDirection.normalized * gc.surfaceSphereCastDist);
        Gizmos.DrawWireSphere(transform.TransformPoint(gc.CheckPoint), gc.CheckRadius);
    }
    private void FixedUpdate()
    {
        CheckGrounding();
    }
    #endregion
}
public enum PlayerState
{
    Still,
    GroundedMotion,
    Slide,

    Jumping,
    Falling,
    Impact,

    Wallrunning,
    Climbing
}