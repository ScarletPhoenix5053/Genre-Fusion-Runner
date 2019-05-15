using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SCARLET.DbOverlay;

[RequireComponent(typeof(PlayerRefs2))]
public class ParkourPlayerController2 : MonoBehaviour
{
    #region Variables

    #region Input and Prefs
    private IPlayerInput inputGroup;
    private PlayerRefs2 refs;
    #endregion

    #region Motion

#pragma warning disable IDE0044
    [Header("General Motion")]
    [SerializeField] [Range(0, 1)] private float instantAcceleration = 0.5f;
    [SerializeField] private float groundFriction = 50f;
    [SerializeField] private float airFriction = 20f;
    [SerializeField] private float strafeForce = 3f;
    [SerializeField] private float airStrafeForce = 2f;
    [SerializeField] private float gravityForce = 2f;
    [Header("Walk")]
    [SerializeField] private float walkForce = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [Header("Sprint")]
    [SerializeField] private float sprintForce;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [Header("Jump")]
    [SerializeField] protected float jumpForce = 12f;
    [SerializeField] private float lateralJumpForce = 2f;
#pragma warning restore IDE0044

    private PlayerMotionControl2 motionControllers;
    private Averager averageFwdSpeedTracker = new Averager(288);
    public float Speed => motionControllers.ActiveMotionController.Speed;
    protected bool Grounded => refs.GroundChecker.Grounded;
    #endregion

    #region Wallrun
#pragma warning disable IDE0044
    [Header("Wallrun Settings")]
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
    [SerializeField] private float wallrunAdjustTime = 0.5f;
    [SerializeField] private float wallrunStartSpeed = 12f;
    [SerializeField] private float wallrunFriction = 35f;
#pragma warning restore IDE0044

    private int wallDirection = 0;
    private WallRunDetector wallRunner;
    private IEnumerator smoothTranslateRoutine;
    #endregion

    #region WallClimb
#pragma warning disable IDE0044
    [Header("Climb")]
    [SerializeField] private LayerMask climbMask;
    [SerializeField] private float maxClimbTime = 2f;
    [SerializeField] private float jumpOffForce = 5f;
    [SerializeField] private float climbSpeed = 5f;
#pragma warning restore IDE0044
    private float climbTime = 0f;
    private bool wallInFront;
    private Collider lastWall;
    #endregion

    #region Slide
#pragma warning disable IDE0044
    [Header("Slide Settings")]
    [SerializeField] private float minSlideSpeed = 1f;
    [SerializeField] private float slideStrafeMaxSpeed = 3f;
    [SerializeField] private float slideStrafeStrength = 0.3f;
    [SerializeField] private float slideFriction = 35f;
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

        // Create and/or init sub-components
        inputGroup = new KBMInputGroup(refs.PlayerPrefrences);
        motionControllers = new PlayerMotionControl2(
            inputGroup,
            new BaseMotionController2[4]
            {
                refs.GroundedMotionController,
                refs.WallrunMotionController,
                refs.SlideMotionController,
                refs.WallClimbMotionController
            });

        // Subscribe to wallrun detector
        wallRunner = GetComponent<WallRunDetector>();
        if (wallRunner == null) Debug.LogWarning("Cannot find a wallrun detector, movement may not work as intended", this);
        else
        {
            wallRunner.OnNewWallDetected += StartWallrun;
            wallRunner.OnDetatchFromWall += EndWallRun;
        }

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
                    //motionControllers.ActiveMotionController.Sprint(sprint);
                    //motionControllers.ActiveMotionController.MoveHorizontal(motion);


                    // temp refs
                    var cf = refs.CoalescingForce;
                    
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
                    var instantAccelThreshold = walkSpeed * instantAcceleration;
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
                        motionForce += ToForce.OverFixedTime(Vector3.forward * sprintForce);
                    }

                    // Limit force??
                    motionForce = Vector3.ClampMagnitude(motionForce, walkSpeed * ToForce.forceMultiplicationFactor);

                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Try jump
                    if (jump && Grounded)
                    {
                        cf.AddForce(ToForce.Instant(Vector3.up * jumpForce));

                        var lateralForce = ToForce.Instant(inputMotion.Flatten().normalized * lateralJumpForce);
                        lateralForce = transform.TransformDirection(lateralForce);
                        cf.AddForce(lateralForce);
                    }

                }

                // Try wallrun
                if (!Grounded && Speed > wallrunMinStartSpeed)
                {
                    wallRunner.CheckForWall();
                }

                // Try slide
                if (Grounded && Speed > minSlideSpeed && crouch)
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

                    // Move
                    //motionControllers.ActiveMotionController.MoveHorizontal(motionAxis);

                    // Create force
                    var motionForce = ToForce.OverFixedTime(motion * wallrunStartSpeed);

                    // Limit force??
                    motionForce = Vector3.ClampMagnitude(motionForce, wallrunStartSpeed * ToForce.forceMultiplicationFactor);
                    
                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Try Jump
                    if (jump)
                    {
                        var jumpDirection = new Vector2(-wallDirection, 0);
                        motionControllers.ActiveMotionController.Jump(jumpDirection);
                        SetStateToNormal();
                        wallRunner.DetatchFromWall();
                    }
                }

                // Detatch from wall if too slow or crouching
                if (Speed <= wallrunMinStaySpeed || crouch)
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
                    Vector3 motionForce = ToForce.OverFixedTime(motion * slideStrafeStrength);

                    // Limit force??
                    motionForce = Vector3.ClampMagnitude(motionForce, slideStrafeMaxSpeed * ToForce.forceMultiplicationFactor);
                    
                    // Apply
                    motionForce = transform.TransformDirection(motionForce);
                    cf.AddForce(motionForce);

                    // Jump
                    if (jump && Grounded)
                    {
                        cf.AddForce(ToForce.Instant(Vector3.up * jumpForce));

                        var lateralForce = ToForce.Instant(inputMotion.Flatten().normalized * lateralJumpForce);
                        lateralForce = transform.TransformDirection(lateralForce);
                        cf.AddForce(lateralForce);

                        SetStateToNormal();
                    }
                }

                // End slide if too slow or airborne
                if (
                    ((Speed < minSlideSpeed) && !inputGroup.GetInputCrouch())
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

                    // Climb
                    motionControllers.ActiveMotionController.MoveHorizontal(inputMotion);
                    cf.AddForce(ToForce.OverFixedTime(Vector3.up * climbSpeed * inputMotion.y));

                    // Jump
                    if (jump)
                    {
                        cf.AddForce(ToForce.Instant(Vector3.up * jumpOffForce));

                        SetStateToNormal();
                    }
                }

                // Manual fall-off
                if (crouch) SetStateToNormal();

                // End climb if 
                // no more wall to climb
                // or climb timer runs out
                climbTime += Time.deltaTime;
                CheckForWallInFront();
                if (!wallInFront || (climbTime > maxClimbTime))
                {
                    SetStateToNormal();
                }

                #endregion
                break;

            // Throw error for unrecognized state
            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }

        UpdateDebugOverlayLogs();

        // track speed
        averageFwdSpeedTracker.Track(refs.CoalescingForce.ForwardVel);
    }
    private void FixedUpdate()
    {
        // If grounded, use ground friction
        if (Grounded && state == CharacterState.Normal)
        {
            refs.Drag.DragConstant = groundFriction;
        }
        else
        {
            refs.Drag.DragConstant = airFriction;
        }
    }
    #endregion

    #region Public Methods



    #endregion
    #region Private Methods

    #region Init
    private void GetRefComponent()
    {
    }
    private void CreateObjectsAndInjectDependencies()
    {
    }
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

        if (newState != CharacterState.Climbing) climbTime = 0;
        state = newState;
    }

    private void SetStateToNormal()
    {
        SetState(CharacterState.Normal);
        motionControllers.SetActiveMotionController(CharacterState.Normal);
        refs.Drag.DragConstant = groundFriction;

        CamControlsRotation(true);
        refs.Cam.DipCamera(false);
    }
    private void SetStateToWallRun()
    {
        SetState(CharacterState.Wallrun);
        motionControllers.SetActiveMotionController(CharacterState.Wallrun);
        refs.Drag.DragConstant = wallrunFriction;

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
        motionControllers.SetActiveMotionController(CharacterState.Slide);

        CamControlsRotation(false);
        refs.Cam.DipCamera(true);
        refs.Drag.DragConstant = slideFriction;
    }
    private void SetStateToWallClimb()
    {
        SetState(CharacterState.Climbing);
        motionControllers.SetActiveMotionController(CharacterState.Climbing);
        refs.Drag.DragConstant = groundFriction;

        // Cancel vertical momentum
        refs.CoalescingForce.ResetVelocityY();

        CamControlsRotation(false);
        refs.Cam.DipCamera(false);
    }
    #endregion

    #region Wallrun
    private void SubscribeToWallrunDetector()
    {
    }
    private void LeapFromWallrun(float fwdSpeed)
    {
        // var lateralBoost = new Vector2(wallrunLeapBoost * -wallDirection, fwdSpeed);
        var lateralBoost = new Vector2(0, wallrunLeapBoost);
        motionControllers.ActiveMotionController.Jump(lateralBoost);
    }

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
        var linearIncriment = Vector3.Distance(to, transform.position) * (wallrunAdjustTime * Time.fixedDeltaTime);

        while (smoothCurrent < wallrunAdjustTime)
        {
            smoothCurrent += wallrunAdjustTime * Time.deltaTime;


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
    private const string idMoveState = "Character Motion Controller";
    private const string idCharSpeed = "Character Speed";
    private const string idCharFwdSpeed = "Character Fwd Speed";

    private static void CreateDebugOverlayLogs()
    {
        DebugOverlay.CreateLog(idCharState);
        DebugOverlay.CreateLog(idMoveState);
        DebugOverlay.CreateLog(idCharSpeed);
        DebugOverlay.CreateLog(idCharFwdSpeed);
    }
    private void UpdateDebugOverlayLogs()
    {
        DebugOverlay.UpdateLog(idCharState, state.ToString());
        DebugOverlay.UpdateLog(idMoveState, motionControllers.ActiveMotionControllerId.ToString());
        DebugOverlay.UpdateLog(idCharSpeed, Speed.ToString());
        DebugOverlay.UpdateLog(idCharFwdSpeed, refs.CoalescingForce.ForwardVel.ToString());
    }
    #endregion
}