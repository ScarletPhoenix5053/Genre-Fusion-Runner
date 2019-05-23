using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SCARLET.DbOverlay;

[RequireComponent(typeof(PlayerRefs2))]
public class ParkourPlayerController2 : MonoBehaviour
{
    #region Variables

    #region Input and Prefs
    [Header("Pref Files")]
    [SerializeField] private PlayerPreferenceGroup prefs;
    [SerializeField] private MotionOptionGroup motion;

    private IPlayerInput inputGroup;
    private PlayerRefs2 refs;
    #endregion

    #region Motion
    public float Speed => refs.CoalescingForce.Speed;
    protected bool Grounded => refs.GroundChecker.Grounded;

    private int airJumpsRemaining = 0;
    #endregion

    #region Wallrun
#pragma warning disable IDE0044
    [Header("Wallrun Quality")]
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunStickTime = 0.5f;

#pragma warning restore IDE0044

    private int wallDirection = 0;
    private WallRunDetector wallRunner;
    private IEnumerator smoothTranslateRoutine;
    #endregion

    #region WallClimb
#pragma warning disable IDE0044
    [Header("Climb")]
    [SerializeField] private LayerMask climbMask;
#pragma warning restore IDE0044
    private float climbTime = 0f;
    private bool wallInFront;
    private Collider lastWall;
    #endregion

    #region Slide
#pragma warning disable IDE0044

    private IEnumerator slideBoostRoutine;
#pragma warning restore IDE0044
    #endregion

    #region State
    private CharacterState state = CharacterState.Normal;
    #endregion

    #endregion

    #region Runtime Messages

    private void Awake()
    {
        // Locate refs component
        refs = GetComponent<PlayerRefs2>();
        Debug.Assert(refs != null);
        Debug.Assert(refs.CoalescingForce != null);

        // Create and/or init sub-components
        inputGroup = new KBMInputGroup(prefs);

        // Subscribe to wallrun detector
        wallRunner = GetComponent<WallRunDetector>();
        if (wallRunner == null) Debug.LogWarning("Cannot find a wallrun detector, movement may not work as intended", this);
        else
        {
            wallRunner.OnNewWallDetected += StartWallrun;
            wallRunner.OnDetatchFromWall += EndWallRun;
        }

        // Subscribe to haiku change
        //HaikuCollectionSystem.OnCompletion += UpdateMotionProfile;

        // Initialize debugging stuff
        CreateDebugOverlayLogs();
    }
    private void Update()
    {
        // Get input
        var inputMotion = inputGroup.GetAxisMotion();
        var sprint = inputGroup.GetInputSprint();
        var jump = inputGroup.GetInputJump();
        var crouch = inputGroup.GetInputCrouch();

        switch (state)
        {
            case CharacterState.Normal:
                #region Normal Motion
                if (Grounded) wallRunner.RestorePrevWall();

                // Camera
                UpdateCam();

                // Move
                {
                    // temp refs
                    var cf = refs.CoalescingForce;
                    var walkSpeed = this.motion.WalkSpeed;
                    var instantAccel = this.motion.InstantWalkAcceleration;
                    var sprintBoost = this.motion.SprintExtraSpeed;
                    var airStrafeForce = this.motion.AirStrafeForce;
                    
                    // Create force vector
                    var motion = new Vector3(inputMotion.x, 0, inputMotion.y);
                    Vector3 motionForce;
                    if (Grounded)
                    {
                        motionForce = ToForce.OverFixedTime(motion * walkSpeed);
                    }
                    else
                    {
                        motionForce = ToForce.OverFixedTime(motion * airStrafeForce);
                    }
                    
                    // Instant accel/decel
                    var instantAccelThreshold = walkSpeed * instantAccel;
                    if (cf.Speed < instantAccelThreshold)
                    {
                        if (inputMotion != Vector2.zero)
                        {
                            cf.SetVelocity(transform.TransformDirection(motion * instantAccelThreshold));
                        }
                        else
                        {
                            cf.ResetVelocityX();
                            cf.ResetVelocityZ();
                        }
                    }

                    // Sprint boost
                    if (Grounded && sprint)
                    {
                        motionForce += ToForce.OverFixedTime(Vector3.forward * sprintBoost);
                    }

                    // Limit force?? 
                    LimitForce(motionForce, to: walkSpeed);

                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Try jump
                    if (jump)
                    {
                        if (Grounded)
                        {
                            Jump(strength: this.motion.GroundedJumpStrength);
                            LateralBoost(inputMotion.Flatten().normalized, this.motion.GroundedJumpBoost);
                        }
                        else if (airJumpsRemaining > 0)
                        {
                            airJumpsRemaining--;
                            cf.ResetVelocityY();
                            Jump(strength: this.motion.GroundedJumpStrength);
                            LateralBoost(inputMotion.Flatten().normalized, this.motion.GroundedJumpBoost);
                        }
                    }

                }

                // Try wallrun
                if (!Grounded && Speed > this.motion.WallrunMinStartSpeed)
                {
                    wallRunner.CheckForWall();
                }

                // Try slide
                if (Grounded && Speed > this.motion.SlideMinSpeed && crouch)
                {
                    SetStateToSlide();
                }

                // Try climb wall
                {
                    var hit = CheckForWallInFront();

                    if (Grounded) lastWall = null;
                    if (
                        wallInFront && inputGroup.GetAxisMotion().y > 0
                        && state != CharacterState.Climbing
                        && hit.collider != lastWall
                        )
                    {
                        lastWall = hit.collider;
                        SetStateToWallClimb();
                    }
                }

                #endregion
                break;

            case CharacterState.Wallrun:
                #region Wallrun

                // Camera
                UpdateCam();

                // Check if still on wall
                wallRunner.CheckForWall(wallDirection);

                {
                    var cf = refs.CoalescingForce;
                    var motion = new Vector3(0, 0, inputMotion.y);
                    var wallrunSpeed = this.motion.WallrunSpeed;

                    // Move
                    //motionControllers.ActiveMotionController.MoveHorizontal(motionAxis);

                    // Create force
                    var motionForce = ToForce.OverFixedTime(motion * wallrunSpeed);

                    // Limit force??
                    motionForce = Vector3.ClampMagnitude(motionForce, wallrunSpeed * ToForce.forceMultiplicationFactor);
                    
                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Try Jump
                    if (jump)
                    {
                        Jump(strength: this.motion.WallJumpStrength);
                        var jumpDirection = new Vector2(-wallDirection, 0);
                        LateralBoost(jumpDirection, this.motion.WallJumpBoost);

                        SetStateToNormal();
                        wallRunner.DetatchFromWall();
                    }
                }

                // Detatch from wall if too slow or crouching
                if (Speed <= this.motion.WallrunMinStaySpeed || crouch)
                {
                    wallRunner.DetatchFromWall();
                }

                #endregion
                break;

            case CharacterState.Slide:
                #region Slide State

                // Camera
                UpdateCam(0.1f);

                {
                    var cf = refs.CoalescingForce;

                    // Strafe
                    //motionControllers.ActiveMotionController.MoveHorizontal(inputMotion);

                    // Create force
                    var motion = new Vector3(inputMotion.x, 0, 0);
                    Vector3 motionForce = ToForce.OverFixedTime(motion * this.motion.SlideStrafeStrength);

                    // Limit force??
                    motionForce = Vector3.ClampMagnitude(motionForce, this.motion.SlideStrafeMaxSpeed * ToForce.forceMultiplicationFactor);
                    
                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Jump
                    if (jump && Grounded)
                    {
                        Jump(strength: this.motion.GroundedJumpStrength);
                        LateralBoost(inputMotion.Flatten().normalized, this.motion.GroundedJumpBoost);

                        SetStateToNormal();
                    }
                }

                // End slide if too slow or airborne
                if (
                    ((Speed < this.motion.SlideMinSpeed) && !inputGroup.GetInputCrouch())
                    || !Grounded
                    )
                {
                    SetStateToNormal();
                }

                #endregion
                break;

            case CharacterState.Climbing:
                #region Climb State
                // Camera
                UpdateCam();

                {
                    var cf = refs.CoalescingForce;

                    // Jump
                    if (jump)
                    {
                        LateralBoost(Vector3.back, this.motion.ClimbJumpForce);
                        Jump(strength: this.motion.ClimbJumpForce);

                        SetStateToNormal();
                    }

                    // Climb
                    cf.AddForce(ToForce.OverFixedTime(Vector3.up * motion.ClimbSpeed * inputMotion.y));

                }

                // Manual fall-off
                if (crouch) SetStateToNormal();

                // End climb if 
                // no more wall to climb
                // or climb timer runs out
                climbTime += Time.deltaTime;
                CheckForWallInFront();
                if (!wallInFront || (climbTime > motion.ClimbMaxDuration))
                {
                    SetStateToNormal();
                }

                #endregion
                break;

            // Throw error for unrecognized state
            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }

        UpdateDebugOverlayLogs();
    }
    private void FixedUpdate()
    {
        // Reset jumps on grounding
        if (Grounded || state == CharacterState.Wallrun) airJumpsRemaining = motion.AirJumps;

        if (state == CharacterState.Normal)
        {
            // If grounded, use ground friction
            if (Grounded && state == CharacterState.Normal)
            {
                refs.Drag.DragConstant = motion.GroundedFriction;
            }
            else
            {
                refs.Drag.DragConstant = motion.AirFriction;
            }            
        }
        // Try grav
        {
            var cf = refs.CoalescingForce;
            if (Grounded || state != CharacterState.Normal) return;

            var gravForce = ToForce.OverFixedTime(Vector3.down * motion.Gravity);
            cf.AddForce(gravForce);
        }
    }
    #endregion

    #region Public Methods

    public MotionOptionGroup GetMotionOptionGroup() => motion;
    public void SetMotionProfile(MotionOptionGroup newMotionProfile)
    {
        motion = newMotionProfile;
    }
    public void UpdateMotionProfile(Haiku haiku)
    {
        //SetMotionProfile(haiku.Scene);
    }

    #endregion
    #region Private Methods
    
    #region Motion
    private void Jump(float strength)
    {
        refs.CoalescingForce.AddForce(ToForce.Instant(Vector3.up * strength));
    }
    private void LateralBoost(Vector2 dir, float strength)
    {
        var lateralForce = ToForce.Instant(dir * strength);
        lateralForce = transform.TransformDirection(lateralForce);
        refs.CoalescingForce.AddForce(lateralForce);
    }
    private Vector3 LimitForce(Vector3 force, float to) => Vector3.ClampMagnitude(force, to * ToForce.forceMultiplicationFactor);
    #endregion

    #region Camera
    private void UpdateCam(float lag = 0f)
    {
        var look = inputGroup.GetAxisLook();
        refs.Cam.Turn(look, lag);
    }
    private void TiltCam(float angle) => refs.Cam.SetTilt(angle);
    private void CamControlsRotation(bool value) => refs.Cam.ControlTargetRotation = value;
    #endregion

    #region State
    private void SetState(CharacterState newState)
    {
        if (newState == state) return;

        // Cancel slide speed boost on change
        if (newState != CharacterState.Slide) refs.CoalescingForce.CancelForceOverTime(slideBoostRoutine);

        // Reset climb time on change
        if (newState != CharacterState.Climbing) climbTime = 0;

        state = newState;
    }

    private void SetStateToNormal()
    {
        SetState(CharacterState.Normal);
        refs.Drag.DragConstant = motion.GroundedFriction;

        CamControlsRotation(true);
        refs.Cam.DipCamera(false);
    }
    private void SetStateToWallRun()
    {
        SetState(CharacterState.Wallrun);
        refs.Drag.DragConstant = this.motion.WallrunFriction;

        // Find new pos & rotation to interpolate to
        SmoothStickToWall();

        // Cancel vertical momentum
        refs.CoalescingForce.ResetVelocityY();

        CamControlsRotation(false);
        refs.Cam.DipCamera(false);
    }
    private void SetStateToSlide()
    {
        SetState(CharacterState.Slide);

        // speed boost
        var startForce = -(ToForce.OverFixedTime(transform.forward * this.motion.SlideStartForce));
        slideBoostRoutine = refs.CoalescingForce.AddForceOverTime(-startForce, this.motion.SlideStartTime);

        CamControlsRotation(false);
        refs.Cam.DipCamera(true);
        refs.Drag.DragConstant = this.motion.SlideFriction;
    }
    private void SetStateToWallClimb()
    {
        SetState(CharacterState.Climbing);
        refs.Drag.DragConstant = motion.GroundedFriction;

        // Cancel vertical momentum
        refs.CoalescingForce.ResetVelocityY();

        CamControlsRotation(false);
        refs.Cam.DipCamera(false);
    }
    #endregion

    #region Wallrun

    private void StartWallrun(BoxCollider newWallTransform, int side)
    {
        wallDirection = side;
        SetStateToWallRun();

        TiltCam(wallrunCamTilt * wallDirection);
    }
    private void EndWallRun()
    {
        //Debug.Log("Ending wallrun");
        wallDirection = 0;
        SetStateToNormal();

        TiltCam(0);
    }

    private void SmoothStickToWall()
    {
        var newRotation = wallRunner.GetWallRunEulers();
        var newPosition = wallRunner.GetWallRunStartPos(refs.MainCollider.radius);
        transform.eulerAngles = newRotation;

        if (smoothTranslateRoutine != null) StopCoroutine(smoothTranslateRoutine);
        smoothTranslateRoutine = SmoothTranslateXRoutine(to: newPosition);
        StartCoroutine(smoothTranslateRoutine);
    }
    private IEnumerator SmoothTranslateXRoutine(Vector3 to)
    {
        var smoothCurrent = 0f;
        var sign = 0;
        var localizedTargetPos = transform.InverseTransformPoint(to);
        if (localizedTargetPos.x <= 0)
        {
            sign = -1;
        }
        else
        {
            sign = 1;
        }
        var linearIncriment = Vector3.Distance(to, transform.position) * (wallrunStickTime * Time.fixedDeltaTime);

        while (smoothCurrent < wallrunStickTime)
        {
            smoothCurrent += wallrunStickTime * Time.deltaTime;


            var interpolatedPos =
                transform.TransformDirection(
                    new Vector3(
                        linearIncriment * sign,
                        0,
                        0
                        ));

            /*
            var interpolatedPos =
                    new Vector3(
                        linearIncriment * sign,
                        0,
                        0
                        );*/

            transform.position += interpolatedPos;
            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator SmoothRotateRoutine()
    {
        throw new System.NotImplementedException();
        /*
        tiltSmoothCurrent = 0;
        prevRotation = transform.rotation;
        targetRotation = Quaternion.Euler(Vector3.forward * targetAngle);

        while (tiltSmoothCurrent < tiltSmoothTime)
        {
            tiltSmoothCurrent += tiltSmoothTime * Time.deltaTime;

            var interpolatedRotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    tiltSmoothCurrent / tiltSmoothTime
                    );

            currentRotation = interpolatedRotation.eulerAngles;
            yield return new WaitForEndOfFrame();
        }*/
    }
    #endregion

    #region Wallclimb
    private RaycastHit CheckForWallInFront()
    {
        var wallCheckDist = .6f;
        var wallRay = new Ray(transform.position, transform.forward);
        wallInFront = Physics.Raycast(wallRay, out RaycastHit hit, wallCheckDist, climbMask, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(transform.position, transform.forward * wallCheckDist);
        return hit;
    }

    #endregion

    #endregion

    #region Debugging
    private const string idCharState = "Character State";
    private const string idCharSpeed = "Character Speed";
    private const string idCharFwdSpeed = "Character Fwd Speed";

    private static void CreateDebugOverlayLogs()
    {
        DebugOverlay.CreateLog(idCharState);
        DebugOverlay.CreateLog(idCharSpeed);
        DebugOverlay.CreateLog(idCharFwdSpeed);
    }
    private void UpdateDebugOverlayLogs()
    {
        DebugOverlay.UpdateLog(idCharState, state.ToString());
        DebugOverlay.UpdateLog(idCharSpeed, Speed.ToString());
        DebugOverlay.UpdateLog(idCharFwdSpeed, refs.CoalescingForce.ForwardVel.ToString());
    }
    #endregion
}
public enum CharacterState
{
    Normal,
    Wallrun,
    Climbing,
    Slide
}