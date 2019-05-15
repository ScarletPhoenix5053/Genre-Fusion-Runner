using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SCARLET.DbOverlay;

[RequireComponent(typeof(PlayerRefs2))]
public class ParkourPlayerController2 : MonoBehaviour
{
    #region Variables

    // INPUT AND PREFS
    private IPlayerInput inputGroup;
    private PlayerRefs2 refs;

    // MOTION
    private PlayerMotionControl2 motionControllers;
    private Averager averageFwdSpeedTracker = new Averager(288);
    public float Speed => averageFwdSpeedTracker.GetAverage();
    protected bool Grounded => refs.GroundChecker.Grounded;


    // WALLRUN
    [Header("Wallrun Settings")]
#pragma warning disable
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
    [SerializeField] private float wallrunAdjustTime = 0.5f;
#pragma warning restore

    private int wallDirection = 0;
    private WallRunDetector wallRunner;
    private IEnumerator smoothTranslateRoutine;


    // WALLCLIMB
    [Header("Climb")]
    [SerializeField] private LayerMask climbMask;
    [SerializeField] private float maxClimbTime = 2f;
    private float climbTime = 0f;
    private bool wallInFront;
    private Collider lastWall;


    // SLIDE
    [Header("Slide Settings")]
    [SerializeField] private float minSlideSpeed = 1f;

    
    // STATE
    private CharacterState state = CharacterState.Normal;

    #endregion

    #region Runtime Messages

    private void Awake()
    {
        GetRefComponent();
        CreateObjectsAndInjectDependencies();
        SubscribeToWallrunDetector();
        CreateOverlayLogs();
    }
    private void Update()
    {
        // Get input
        var motion = inputGroup.GetAxisMotion();
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
                motionControllers.ActiveMotionController.Sprint(sprint);
                motionControllers.ActiveMotionController.MoveHorizontal(motion);

                // Try Jump
                if (jump && Grounded) motionControllers.ActiveMotionController.Jump(motion);

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
                    var wallCheckDist = .6f;
                    var wallRay = new Ray(transform.position, transform.forward);
                    RaycastHit hit;
                    wallInFront = Physics.Raycast(wallRay, out hit, wallCheckDist, climbMask, QueryTriggerInteraction.Ignore);
                    Debug.DrawRay(transform.position, transform.forward * wallCheckDist);

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

                // Move
                var motionAxis = inputGroup.GetAxisMotion();
                motionControllers.ActiveMotionController.MoveHorizontal(motionAxis);

                // Try Jump
                if (jump)
                {
                    var jumpDirection = new Vector2(-wallDirection, 0);
                    motionControllers.ActiveMotionController.Jump(jumpDirection);
                    SetStateToNormal();
                    wallRunner.DetatchFromWall();
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

                // Strafe
                motionControllers.ActiveMotionController.MoveHorizontal(motion);

                // Jump
                if (jump)
                {
                    motionControllers.ActiveMotionController.Jump(motion);
                    SetStateToNormal();
                }

                // End slide if too slow or airborne
                if (
                    ((motionControllers.ActiveMotionController.Speed < minSlideSpeed) && !inputGroup.GetInputCrouch())
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

                // Climb
                motionControllers.ActiveMotionController.MoveHorizontal(motion);

                // Jump off
                if (jump)
                {
                    motionControllers.ActiveMotionController.Jump();
                    SetStateToNormal();
                }

                // Manual fall-off
                if (crouch) SetStateToNormal();

                // End climb if 
                // no more wall to climb
                // or climb timer runs out
                climbTime += Time.deltaTime;
                if (!wallInFront || (climbTime > maxClimbTime))
                {
                    SetStateToNormal();
                }

                #endregion
                break;

            // Throw error for unrecognized state
            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }

        UpdateOverlayLogs();

        // track speed
        averageFwdSpeedTracker.Track(refs.CoalescingForce.ForwardVel);
    }

    #endregion

    #region Public Methods



    #endregion

    #region Private Methods

    // INIT
    private void GetRefComponent()
    {
        refs = GetComponent<PlayerRefs2>();
    }
    private void CreateObjectsAndInjectDependencies()
    {
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
    }


    // CAMERA
    private void UpdateCam(float lag = 0f)
    {
        var look = inputGroup.GetAxisLook();
        refs.Cam.Turn(look, lag);
    }
    private void TiltCam(float angle) => refs.Cam.SetTilt(angle);
    private void CamControlsRotation(bool value) => refs.Cam.ControlTargetRotation = value;


    // STATE
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

        CamControlsRotation(true);
        refs.Cam.DipCamera(false);
    }
    private void SetStateToWallRun()
    {
        SetState(CharacterState.Wallrun);
        motionControllers.SetActiveMotionController(CharacterState.Wallrun);

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
    }
    private void SetStateToWallClimb()
    {
        SetState(CharacterState.Climbing);
        motionControllers.SetActiveMotionController(CharacterState.Climbing);

        // Cancel vertical momentum
        refs.CoalescingForce.ResetVelocityY();

        CamControlsRotation(false);
        refs.Cam.DipCamera(false);
    }


    // WALLRUN
    private void SubscribeToWallrunDetector()
    {
        wallRunner = GetComponent<WallRunDetector>();
        if (wallRunner == null) Debug.LogWarning("Cannot find a wallrun detector, movement may not work as intended", this);
        else
        {
            wallRunner.OnNewWallDetected += StartWallrun;
            wallRunner.OnDetatchFromWall += EndWallRun;
        }
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

    #region Debugging
    private const string idCharState = "Character State";
    private const string idMoveState = "Character Motion Controller";
    private const string idCharSpeed = "Character Speed";
    private const string idCharFwdSpeed = "Character Fwd Speed";
    private static void CreateOverlayLogs()
    {
        DebugOverlay.CreateLog(idCharState);
        DebugOverlay.CreateLog(idMoveState);
        DebugOverlay.CreateLog(idCharSpeed);
        DebugOverlay.CreateLog(idCharFwdSpeed);
    }

    private void UpdateOverlayLogs()
    {
        DebugOverlay.UpdateLog(idCharState, state.ToString());
        DebugOverlay.UpdateLog(idMoveState, motionControllers.ActiveMotionControllerId.ToString());
        DebugOverlay.UpdateLog(idCharSpeed, Speed.ToString());
        DebugOverlay.UpdateLog(idCharFwdSpeed, refs.CoalescingForce.ForwardVel.ToString());
    }
    #endregion
}