using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundCheckerSimple))]
public class PhysicsParkourController : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("Refrences")]
    [SerializeField] private PlayerPreferenceGroup preferences;
    [SerializeField] private CameraController cam;
    [Header("Grounded Movement Options")]
    [SerializeField] private float walkForce = 1000f;
    [SerializeField] private float runForce = 3000f;
    [Header("Air Movement Options")]
    [SerializeField] private float jumpLaunchForce = 2000f;
    [SerializeField] private float airStrafeForce = 500f;
    /*
    [Header("Wallrun Settings")]
    [SerializeField] private float wallrunCamTilt = 5f;
    [SerializeField] private float wallrunMinStartSpeed = 5f;
    [SerializeField] private float wallrunMinStaySpeed = 0f;
    [SerializeField] private float wallrunLeapBoost = 10f;
    */
#pragma warning restore 0649
    #endregion
    #region Refrences
    private IPlayerInput inputGroup;
    private Rigidbody rb;
    private GroundCheckerSimple groundChecker;
    #endregion
    #region Private Vars
    private PCharacterState majorState;
    #endregion
    #region Properites
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
    }
    private void Update()
    {
        // Move camera
        UpdateCam();

        switch (majorState)
        {
            case PCharacterState.Grounded:
                // Jump options
                if (inputGroup.GetInputJump())
                {
                    Jump();
                    SetState(PCharacterState.Airborne);
                }
                break;

            case PCharacterState.Airborne:
                break;

            default: goto case PCharacterState.Grounded;
        }
    }
    private void FixedUpdate()
    {
        #region Init
        var motionInput = inputGroup.GetAxisMotion();
        var motionVector = new Vector3(motionInput.x, 0, motionInput.y);
        #endregion

        switch (majorState)
        {
            case PCharacterState.Grounded:
                #region Grounded State
                // Move player
                // Prevent extrenal wind forces moving this object??
                // Keep player level on the ground.
                rb.AddForce(transform.TransformDirection(motionVector) * walkForce * Time.fixedDeltaTime);
                #endregion
                break;

            case PCharacterState.Airborne:
                #region Airborne State
                // Apply less fore from motion
                // Apply gravity force
                // Allow external wind forces to act on this object
                rb.AddForce(transform.TransformDirection(motionVector) * airStrafeForce * Time.fixedDeltaTime);

                // Gravity and landing
                if (groundChecker.Grounded)
                {
                    SetState(PCharacterState.Grounded);
                }
                #endregion
                break;

            default: goto case PCharacterState.Grounded;
        }
    }
    #endregion

    #region Methods
    private void AssignRefrences()
    {
        // REQUIRED COMPONENTS        
        rb = GetComponent<Rigidbody>();
        groundChecker = GetComponent<GroundCheckerSimple>();

        // GLOBAL
        // Prefs
        if (preferences == null) Debug.LogError("Cannot find player prefrences: unable to complete validataion.", this);

        // Camera
        if (cam == null)
        {
            if (!(Camera.main != null && (cam = Camera.main.GetComponent<CameraController>())))
            Debug.LogError("Cannot find camera: unable to complete validataion.", this);
        }
    }
    private void SetState(PCharacterState newState)
    {
        if (newState == majorState) return;

        majorState = newState;
    }
    

    private void UpdateCam()
    {
        // Camera
        var look = inputGroup.GetAxisLook();
        cam.Turn(look);
    }

    private void Jump()
    {
        rb.AddForce(transform.up * jumpLaunchForce);
    }
    #endregion
}
public enum PCharacterState
{
    Grounded,
    Airborne,
    Wallrunning,
    Sliding
}