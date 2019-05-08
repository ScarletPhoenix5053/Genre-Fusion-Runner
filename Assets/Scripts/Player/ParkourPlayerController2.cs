using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SCARLET.DbOverlay;

[RequireComponent(typeof(PlayerRefs2))]
public class ParkourPlayerController2 : MonoBehaviour
{
    #region Flow
    private void Awake()
    {
        GetRefComponent();
        CreateObjectsAndInjectDependencies();
        SubscribeToWallrunDetector();
        CreateOverlayLogs();
    }
    private void Update()
    {
        // Create generic motion interface for each state
        // motion controllers handle juggling of control between themselves?
        // PlayerControl > Motion Control Handler > Indv motion controllers > Translator ????

        switch (state)
        {
            case CharacterState.Normal: LoopNormalState(); break;
            case CharacterState.Wallrun: LoopWallrunState(); break;
            case CharacterState.Slide: LoopSlideState(); break;
            case CharacterState.Climbing: LoopWallClimbState(); break;

            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }

        UpdateOverlayLogs();

        // track speed
        averageFwdSpeedTracker.Track(refs.CoalescingForce.ForwardVel);
    }
    #endregion

    /*  PROBLEM AREAS:
     *  
     *  Momentum - momentum needs to carry accross between motion controllers and translator
     *      Force-based translator?
     *      
     *      PLAYER CONTROLLER
     *          \ 
     *          MOTION CONTROLLERS
     *              \
     *              FORCE-BASED TRANSLATOR
     *                  \
     *                  TRANSFORM.POSITION
     *      
     *  Individual state control - simplifty interface with motion controllers
     *  Motion controller and player sates operate wtih similar enums
     *  
     *  Extensive wallrun code - separate into wallrun classes or make new ones
     */

    #region Cam
    private void UpdateCam(float lag = 0f)
    {
        var look = inputGroup.GetAxisLook();
        refs.Cam.Turn(look, lag);
    }
    private void TiltCam(float angle) => refs.Cam.SetTilt(angle);
    private void CamControlsRotation(bool value) => refs.Cam.ControlTargetRotation = value;
    #endregion
    #region State Control
    private void SetState(CharacterState newState)
    {
        if (newState == state) return;

        if (newState != CharacterState.Climbing) climbTime = 0;
        state = newState;
    }
    private CharacterState state = CharacterState.Normal;
    private PlayerMotionControl2 motionControllers;
    #endregion
    #region Normal Motion
    private void SetStateToNormal()
    {
        SetState(CharacterState.Normal);
        motionControllers.SetActiveMotionController(CharacterState.Normal);

        CamControlsRotation(true);
        refs.Cam.DipCamera(false);
    }
    private void MoveAsNormal()
    {
        var motion = inputGroup.GetAxisMotion();
        var sprint = inputGroup.GetInputSprint();
        var jump = inputGroup.GetInputJump();
        var crouch = inputGroup.GetInputCrouch();
        TryNormalMotion(motion, sprint, jump);
        TryWallRun();
        TrySlide(crouch);
        TryWallClimb();
    }
    private void LoopNormalState()
    {
        UpdateCam();
        AllowRunningOnPrevWall();
        MoveAsNormal();
    }
    private void TryNormalMotion(Vector2 motion, bool sprint, bool jump)
    {
        motionControllers.ActiveMotionController.Sprint(sprint);
        if (jump && Grounded) motionControllers.ActiveMotionController.Jump(motion);
        motionControllers.ActiveMotionController.MoveHorizontal(motion);
    }

    private Averager averageFwdSpeedTracker = new Averager(288);
    public float Speed => averageFwdSpeedTracker.GetAverage();
    protected bool Grounded => refs.GroundChecker.Grounded;
    #endregion
    #region Wallrunning
    private void AllowRunningOnPrevWall()
    {
        if (Grounded) wallRunner.RestorePrevWall();
    }
    #region Inspector
    [Header("Wallrun Settings")]
#pragma warning disable
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
    [SerializeField] private float wallrunAdjustTime = 0.5f;
#pragma warning restore
    #endregion
    private int wallDirection = 0;
    private WallRunDetector wallRunner;

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

    private void LoopWallrunState()
    {
        CheckIfStillOnWall();
        UpdateCam();
        MoveAsWallrunning();
    }
    private void CheckIfStillOnWall()
    {
        wallRunner.CheckForWall(wallDirection);
    }

    private void MoveAsWallrunning()
    {
        var motion = inputGroup.GetAxisMotion();
        motionControllers.ActiveMotionController.MoveHorizontal(motion);

        var speed = motionControllers.ActiveMotionController.Speed;
        TryWallJump();
        DetatchIfTooSlowOrCrouching();
    }
    private void DetatchIfTooSlowOrCrouching()
    {
        // Detatch if too slow or crouching
        
        var crouch = inputGroup.GetInputCrouch();
        if (Speed <= wallrunMinStaySpeed || crouch)
        {
            //if (crouch) Debug.Log("Crouch input: ending wallrun");
            //else Debug.Log("Too slow to maintain wallrun");
            wallRunner.DetatchFromWall();
        }
    }
    private void TryWallJump()
    {
        // Leap off of wall if jump input
        var jump = inputGroup.GetInputJump();
        if (jump)
        {
            var jumpDirection = new Vector2(-wallDirection, 0);
            motionControllers.ActiveMotionController.Jump(jumpDirection);
            SetStateToNormal();
            wallRunner.DetatchFromWall();
        }
    }

    private void TryWallRun()
    {

        // If able to wallrun
        if (!Grounded && Speed > wallrunMinStartSpeed)
        {
            // Check for wall to run on
            wallRunner.CheckForWall();
        }
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

    private void LeapFromWallrun(float fwdSpeed)
    {
        // var lateralBoost = new Vector2(wallrunLeapBoost * -wallDirection, fwdSpeed);
        var lateralBoost = new Vector2(0, wallrunLeapBoost);
        motionControllers.ActiveMotionController.Jump(lateralBoost);
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
    private IEnumerator smoothTranslateRoutine;
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
    #region Sliding 
#pragma warning disable
    [SerializeField] private float minSlideSpeed = 1f;
#pragma warning restore
    private void SetStateToSlide()
    {
        SetState(CharacterState.Slide);
        motionControllers.SetActiveMotionController(CharacterState.Slide);

        CamControlsRotation(false);
        refs.Cam.DipCamera(true);
    }

    private void TrySlide(bool crouch)
    {
        // If running and holding crouch
        if (Grounded && Speed > minSlideSpeed && crouch)
        {
            // Slide!
            SetStateToSlide();
        }
    }
    private void LoopSlideState()
    {
        UpdateCam(0.1f);
        MoveAsSliding();
    }
    private void MoveAsSliding()
    {
        var motion = inputGroup.GetAxisMotion();
        var jump = inputGroup.GetInputJump();
        TryStrafe(motion);
        TryJumpFromSlide(jump, motion);
        EndSlideIfTooSlowOrAirborne();
    }
    private void EndSlideIfTooSlowOrAirborne()
    {
        if (((motionControllers.ActiveMotionController.Speed < minSlideSpeed) && !inputGroup.GetInputCrouch()) || !Grounded)
        {
            SetStateToNormal();
        }
    }
    private void TryJumpFromSlide(bool jump, Vector2 motion)
    {
        if (jump)
        {
            motionControllers.ActiveMotionController.Jump(motion);
            SetStateToNormal();
        }
    }
    private void TryStrafe(Vector2 motion)
    {
        motionControllers.ActiveMotionController.MoveHorizontal(motion);
    }
    #endregion
    #region WallClimb
    [Header("Climb")]
    [SerializeField] private LayerMask climbMask;
    [SerializeField] private float maxClimbTime = 2f;
    private float climbTime = 0f;
    private bool wallInFront;
    private Collider lastWall;
    private void TryWallClimb()
    {
        var wallCheckDist = .6f;
        var wallRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        wallInFront = Physics.Raycast(wallRay, out hit, wallCheckDist, climbMask, QueryTriggerInteraction.Ignore);
        Debug.DrawRay(transform.position, transform.forward * wallCheckDist);
        
        if (Grounded) lastWall = null;
        if (wallInFront && inputGroup.GetAxisMotion().y > 0 && state != CharacterState.Climbing && hit.collider != lastWall)
        {
            lastWall = hit.collider;
            SetStateToWallClimb();
        }
    }
    private void SetStateToWallClimb()
    {
        SetState(CharacterState.Climbing);
        motionControllers.SetActiveMotionController(CharacterState.Climbing);

        // Find new pos & rotation to interpolate to
        //SmoothStickToWall();

        // Cancel vertical momentum
        refs.CoalescingForce.ResetVelocityY();

        CamControlsRotation(false);
        refs.Cam.DipCamera(false);
    }

    private void LoopWallClimbState()
    {
        UpdateCam();
        MoveAsWallClimb();
        TryWallClimb();

        climbTime += Time.deltaTime;
        if (climbTime > maxClimbTime)
        {
            SetStateToNormal();
        }
    }

    private void MoveAsWallClimb()
    {
        var motion = inputGroup.GetAxisMotion();
        var sprint = inputGroup.GetInputSprint();
        var jump = inputGroup.GetInputJump();
        var crouch = inputGroup.GetInputCrouch();

        motionControllers.ActiveMotionController.MoveHorizontal(motion);
        if (jump)
        {
            motionControllers.ActiveMotionController.Jump();
            SetStateToNormal();
        }
        if (crouch) SetStateToNormal();

        if (!wallInFront) SetStateToNormal();
    }
    #endregion
    #region Input and Prefs
    private IPlayerInput inputGroup;
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
    #region Init
    private PlayerRefs2 refs;
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
    #endregion    
}