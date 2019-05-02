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

            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }

        // Update overlay
        DebugOverlay.UpdateLog("Character State", state.ToString());
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
    private void UpdateCam()
    {
        var look = inputGroup.GetAxisLook();
        refs.Cam.Turn(look);
    }
    private void TiltCam(float angle) => refs.Cam.SetTilt(angle);
    private void CamControlsRotation(bool value) => refs.Cam.ControlTargetRotation = value;
    #endregion
    #region State Control
    private void TryWallRun()
    {

        // If able to wallrun
        if (!Grounded && Speed > wallrunMinStartSpeed)
        {
            // Check for wall to run on
            wallRunDetector.CheckForWall();
        }
    }
    private void SetStateToSlide()
    {
        SetState(CharacterState.Slide);
        motionControllers.SetActiveMotionController(CharacterState.Slide);

        CamControlsRotation(false);
    }
    private void SetStateToWallRun()
    {
        SetState(CharacterState.Wallrun);
        //motionControllers.SetActiveMotionController(CharacterState.Wallrun);

        // Find new pos & rotation to interpolate to
        SmoothStickToWall();

        CamControlsRotation(false);
    }
    private void SetState(CharacterState newState)
    {
        if (newState == state) return;

        state = newState;
    }
    private void SetStateToNormal()
    {
        SetState(CharacterState.Normal);
        motionControllers.SetActiveMotionController(CharacterState.Normal);

        CamControlsRotation(true);
    }
    private CharacterState state = CharacterState.Normal;
    private PlayerMotionControl2 motionControllers;
    #endregion
    #region Normal Motion
    private void MoveAsNormal()
    {
        var motion = inputGroup.GetAxisMotion();
        var sprint = inputGroup.GetInputSprint();
        var jump = inputGroup.GetInputJump();
        var crouch = inputGroup.GetInputCrouch();
        TryNormalMotion(motion, sprint, jump);
        //TryWallRun();
        TrySlide(crouch);
    }
    private void LoopNormalState()
    {
        UpdateCam();
        //AllowRunningOnPrevWall();
        MoveAsNormal();
    }
    private void TryNormalMotion(Vector2 motion, bool sprint, bool jump)
    {
        //motionControllers.ActiveMotionController.Sprint(sprint);
        if (jump && Grounded) motionControllers.ActiveMotionController.Jump(motion);
        motionControllers.ActiveMotionController.MoveHorizontal(motion);
    }

    public float Speed => motionControllers.ActiveMotionController.Speed;
    protected bool Grounded => refs.GroundChecker.Grounded;
    #endregion
    #region Wallrunning
    private void EndWallRun()
    {
        //Debug.Log("Ending wallrun");
        wallDirection = 0;
        SetStateToNormal();

        TiltCam(0);
    }
    private void LoopWallrunState()
    {
        CheckIfStillOnWall();
        UpdateCam();
        MoveAsWallrunning();
    }
    private void MoveAsWallrunning()
    {
        var motion = inputGroup.GetAxisMotion();
        motionControllers.ActiveMotionController.MoveHorizontal(motion);

        var speed = motionControllers.ActiveMotionController.Speed;
        TryWallJump(speed);
        DetatchIfTooSlowOrCrouching(speed);
    }
    private void DetatchIfTooSlowOrCrouching(float speed)
    {

        // Detatch if too slow or crouching
        var crouch = inputGroup.GetInputCrouch();
        if (speed <= wallrunMinStaySpeed || crouch)
        {
            //if (crouch) Debug.Log("Crouch input: ending wallrun");
            //else Debug.Log("Too slow to maintain wallrun");
            wallRunDetector.DetatchFromWall();
        }
    }
    private void TryWallJump(float speed)
    {

        // Leap off of wall if jump input
        var jump = inputGroup.GetInputJump();
        if (jump)
        {
            //Debug.Log("Leaping from wall");
            SetStateToNormal();
            LeapFromWallrun(speed);
            wallRunDetector.DetatchFromWall();
        }
    }
    private void CheckIfStillOnWall()
    {
        wallRunDetector.CheckForWall(wallDirection);
    }
    private void AllowRunningOnPrevWall()
    {
        if (Grounded) wallRunDetector.RestorePrevWall();
    }
    [Header("Wallrun Settings")]
    // New class for handling tilt & wallrun adjustments
#pragma warning disable
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
    [SerializeField] private float wallrunAdjustTime = 0.5f;
#pragma warning restore

    private IEnumerator smoothTranslateRoutine;
    private int wallDirection = 0;
    private WallRunDetector wallRunDetector;

    private void SubscribeToWallrunDetector()
    {
        wallRunDetector = GetComponent<WallRunDetector>();
        if (wallRunDetector == null) Debug.LogWarning("Cannot find a wallrun detector, movement may not work as intended", this);
        else
        {
            wallRunDetector.OnNewWallDetected += StartWallrun;
            wallRunDetector.OnDetatchFromWall += EndWallRun;
        }
    }
    private void SmoothStickToWall()
    {
        var newRotation = wallRunDetector.GetWallRunEulers();
        var newPosition = wallRunDetector.GetWallRunStartPos(refs.MainCollider.radius);
        transform.eulerAngles = newRotation;

        if (smoothTranslateRoutine != null) StopCoroutine(smoothTranslateRoutine);
        smoothTranslateRoutine = SmoothTranslateXRoutine(to: newPosition);
        StartCoroutine(smoothTranslateRoutine);
    }
    private void StartWallrun(BoxCollider newWallTransform, int side)
    {
        wallDirection = side;
        SetStateToWallRun();

        TiltCam(wallrunCamTilt * wallDirection);
    }
    private void LeapFromWallrun(float fwdSpeed)
    {
        // var lateralBoost = new Vector2(wallrunLeapBoost * -wallDirection, fwdSpeed);
        var lateralBoost = new Vector2(0, wallrunLeapBoost);
        motionControllers.ActiveMotionController.Jump(lateralBoost);
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
    #region Sliding 
#pragma warning disable
    [SerializeField] private float minSlideSpeed = 1f;
#pragma warning restore
    private void TrySlide(bool crouch)
    {

        // If running and holding crouch
        var minSlideSpeed = wallrunMinStartSpeed;
        if (Grounded && Speed > minSlideSpeed && crouch)
        {
            // Slide!
            SetStateToSlide();
        }
    }
    private void LoopSlideState()
    {
        UpdateCam();
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
    #region Input and Prefs
    private IPlayerInput inputGroup;
    #endregion
    #region Debugging
    private const string idCharState = "Character State";
    private static void CreateOverlayLogs()
    {
        DebugOverlay.CreateLog(idCharState);
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
            new BaseMotionController2[2]
            {
                refs.GroundedMotionController,
                refs.SlideMotionController
            });
    }
    #endregion    
}