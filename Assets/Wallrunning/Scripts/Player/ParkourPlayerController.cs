using UnityEngine;
using System.Collections.Generic;

public class ParkourPlayerController : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("Refrences")]
    [SerializeField] private PlayerPreferenceGroup preferences;
    [SerializeField] private CameraController cam;
    [SerializeField] private GroundChecker gravityChecker;
    [SerializeField] private List<BaseMotionController> motionControllers = new List<BaseMotionController>();
    [SerializeField] private CapsuleCollider capsuleCollider;
    [Header("Wallrun Settings")]
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
#pragma warning restore 0649
    #endregion
    #region Private Vars
    private IPlayerInput inputGroup;
    private CharacterState state = CharacterState.Normal;

    private WallRunDetector wallRunDetector;

    private int activeMotionIndex = 0;
    private const int groundMotionIndex = 0;
    private const int wallrunMotionIndex = 1;

    private int wallDirection = 0;
    #endregion
    #region Properites
    public float Speed => motionControllers[activeMotionIndex].Speed;
    protected bool Grounded => gravityChecker.Grounded;
    #endregion

    #region Unity Messages
    private void Reset()
    {
        AssignRefrences();
    }
    private void Awake()
    {
        AssignRefrences();

        // Create objects and inject dependencies
        inputGroup = new KBMInputGroup(preferences);

        // Subscribe to wallrundetector
        wallRunDetector = GetComponent<WallRunDetector>();
        if (wallRunDetector == null) Debug.LogWarning("Cannot find a wallrun detector, movement may not work as intended", this);
        else
        {
            wallRunDetector.OnNewWallDetected += StartWallrun;
            wallRunDetector.OnDetatchFromWall += EndWallRun;
        }
    }
    private void Update()
    {
        switch (state)
        {
            case CharacterState.Normal: LoopNormalState(); break;
            case CharacterState.Wallrunning: LoopWallrunState(); break;

            default: throw new System.NotImplementedException("Please impliment interactions for state: " + state.ToString());
        }
        
    }
    #endregion

    #region Methods
    private void AssignRefrences()
    {
        // Prefs
        if (preferences == null) Debug.LogError("Cannot find player prefrences: unable to complete validataion.", this);

        // Camera
        if (cam == null)
        {
            if (!(Camera.main != null && (cam = Camera.main.GetComponent<CameraController>())))
            Debug.LogError("Cannot find camera: unable to complete validataion.", this);
        }

        // Gravity
        if (gravityChecker == null) gravityChecker = Helper.FindRelevantComponent<GroundChecker>(transform);

        // Colliders
        if (capsuleCollider == null) capsuleCollider = Helper.FindRelevantComponent<CapsuleCollider>(transform);
    }
    private void SetState(CharacterState newState)
    {
        if (newState == state) return;

        state = newState;
    }

    private void LoopNormalState()
    {
        UpdateCam();

        // Allow use of previously touched wall once grounded
        if (Grounded) wallRunDetector.RestorePrevWall();

        // Base Move
        var motion = inputGroup.GetAxisMotion();
        var sprint = inputGroup.GetInputSprint();
        var jump = inputGroup.GetInputJump();

        motionControllers[groundMotionIndex].Sprint(sprint);
        if (jump && Grounded) motionControllers[groundMotionIndex].Jump();
        motionControllers[groundMotionIndex].MoveHorizontal(motion);

        var speed = motionControllers[groundMotionIndex].Speed;

        // If able to wallrun
        if (motion.x != 0 && speed > wallrunMinStartSpeed)
        {
            // Check for wall to run on
            var strafeSign = System.Math.Sign(motion.x);
            wallRunDetector.CheckForWall(strafeSign);
        }
    }
    private void LoopWallrunState()
    {
        // Check if still on wall
        wallRunDetector.CheckForWall(wallDirection);

        // Cam
        UpdateCam();

        // Move using wallrun controller
        var motion = inputGroup.GetAxisMotion();        
        motionControllers[wallrunMotionIndex].MoveHorizontal(motion);

        var speed = motionControllers[wallrunMotionIndex].Speed;

        // Leap off of wall if jump input
        var jump = inputGroup.GetInputJump();
        if (jump)
        {
            Debug.Log("Leaping from wall");
            SetStateToNormal();
            LeapFromWallrun(speed);
            wallRunDetector.DetatchFromWall();
        }
        // Detatch if too slow or crouching
        var crouch = inputGroup.GetInputCrouch();
        if (speed <= wallrunMinStaySpeed || crouch)
        {
            if (crouch) Debug.Log("Crouch input: ending wallrun");
            else Debug.Log("Too slow to maintain wallrun");
            wallRunDetector.DetatchFromWall();
        }
    }

    private void LeapFromWallrun(float fwdSpeed)
    {
       // var lateralBoost = new Vector2(wallrunLeapBoost * -wallDirection, fwdSpeed);
        var lateralBoost = new Vector2(0, wallrunLeapBoost);
        motionControllers[groundMotionIndex].Jump(lateralBoost);
    }
    private void SetStateToNormal()
    {
        SetState(CharacterState.Normal);
        cam.ControlTargetRotation = true;
        motionControllers[groundMotionIndex].enabled = true;
        motionControllers[wallrunMotionIndex].enabled = false;
        activeMotionIndex = groundMotionIndex;

        // Un-tilt cam
        cam.SetTilt(0);

    }
    private void SetStateToWallRun()
    {
        activeMotionIndex = wallrunMotionIndex;

        SetState(CharacterState.Wallrunning);
        cam.ControlTargetRotation = false;
        motionControllers[wallrunMotionIndex].enabled = true;
        motionControllers[groundMotionIndex].enabled = false;


        // Find new pos & rotation to lock to
        var newRotation = wallRunDetector.GetWallRunEulers();
        var newPosition = wallRunDetector.GetWallRunStartPos(capsuleCollider.radius);
        transform.eulerAngles = newRotation;
        transform.position = newPosition;

        // Tilt cam
        cam.SetTilt(wallrunCamTilt * wallDirection);
    }

    private void UpdateCam()
    {
        // Camera
        // postponing fix untill I update script
        var look = inputGroup.GetAxisLook();
        cam.Turn(look);
    }
    private void StartWallrun(BoxCollider newWallTransform, int side)
    {
        Debug.Log("Wallrun on " + newWallTransform + ". Direction: " + side);
        wallDirection = side;
        SetStateToWallRun();
    }
    private void EndWallRun()
    {
        //Debug.Log("Ending wallrun");
        wallDirection = 0;
        SetStateToNormal();
    }
    #endregion
}
public enum CharacterState
{
    Normal,
    Wallrunning,
    Sliding,
    Climbing
}