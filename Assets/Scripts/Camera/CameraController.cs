using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("Refrences")]
    [SerializeField] private Transform target;
    [Header("Position")]
    [SerializeField] private Vector3 offset = Vector3.up;
    [SerializeField] private float distFromTarget = 8.5f;
    [Header("Limits")]
    [SerializeField] private Vector2 pitchMinMax = new Vector2(-48, 85);
    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.12f;
    [SerializeField] private float tiltSmoothTime = 0.5f;
#pragma warning restore 0649
    #endregion
    #region Private Vars
    private Vector3 rotationSmoothVel;
    private Vector3 currentRotation;

    private IEnumerator tiltRoutine;
    private Quaternion prevRotation;
    private Quaternion targetRotation;
    private float targetAngle = 0f;

    private float yaw;
    private float pitch;
    private float tiltSmoothCurrent;
    #endregion

    public bool ControlTargetRotation { get; set; } = true;

    private void Awake()
    {
        offsetOnAwake = offset;
        dipTarget = offsetOnAwake.y;
    }
    private void Update()
    {
        SmoothTowardsTargetTilt();
        SmoothTowardsTargetTiltOffset();
        SmoothTowardsTargetDip();
    }
    public void Turn(Vector2 input, float lagTime = 0f)
    {
        // Create rotation values
        yaw += input.x;
        pitch -= input.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // Apply rotation
        Debug.Log("Cam: " + currentRotation);
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw, currentRotation.z), ref rotationSmoothVel, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // Control target rotation
        //this.lagTime = lagTime;
        SmoothTargetRotation();

        // Move position
        transform.position = (target.position - transform.forward * distFromTarget) + transform.TransformDirection(new Vector3(offset.x, 0, offset.z)) + Vector3.up * offset.y;
    }
    public void AdjustTarget(Vector3 rot)
    {
        Debug.Log("Cam adjust");
        Debug.Log("Cam before: " + currentRotation);
        transform.eulerAngles = rot;
        currentRotation = rot;
        yaw = rot.y;
        pitch = rot.x;
        Debug.Log("Cam after: " + currentRotation);
    }

    float lagTime = 0f;
    float targetRotSmoothVel = 0f;
    private void SmoothTargetRotation()
    {
        if (ControlTargetRotation)
        {
            Vector3 e;

            e.x = 0;
            e.z = 0;

            if (lagTime == 0)
            {
                e.y = Mathf.SmoothDampAngle(
                    target.eulerAngles.y,
                    transform.eulerAngles.y,
                    ref targetRotSmoothVel,
                    lagTime
                    );
            }
            else
            {
                e.y = transform.eulerAngles.y;
            }

            target.eulerAngles = e;
        }
    }

    #region Camera Tilt
    [SerializeField] private float pushOutDistance = 1f;
    private float pushOutTarget = 0f;
    private float pushOutVel = 0f;

    public void SetTilt(float angle)
    {
        // Adjust interpolation angle        
        tiltTarget = angle;

        // Adjust interpolation pos 
        if (angle != 0f)
        {
            // offset x not behacing as expected on walls. 
            var sign = Mathf.Sign(tiltTarget);
            pushOutTarget = sign < 0 ? offsetOnAwake.x + pushOutDistance : offsetOnAwake.x - pushOutDistance;
        }
        else
        {
            pushOutTarget = offsetOnAwake.x;
        }
    }
    private float tiltTarget = 0f;
    private float tiltSmoothVel = 0f;
    private void SmoothTowardsTargetTilt()
    {
        currentRotation.z = Mathf.SmoothDampAngle(
            currentRotation.z,
            tiltTarget,
            ref tiltSmoothVel,
            tiltSmoothTime
            );
    }
    private void SmoothTowardsTargetTiltOffset()
    {
        offset.x = Mathf.SmoothDampAngle(
            offset.x,
            pushOutTarget,
            ref pushOutVel,
            tiltSmoothTime
            );
    }

    #endregion
    #region Camera Dip
    [SerializeField] private float cameraDip = 1f;
    [SerializeField] private float cameraDipTime = 0.2f;
    private float dipTarget = 1f;
    private float dipSmoothVel = 0f;
    private Vector3 offsetOnAwake;

    public void DipCamera(bool dip)
    {
        dipTarget = dip ? offsetOnAwake.y - cameraDip : offsetOnAwake.y - 0;
    }
    private void SmoothTowardsTargetDip()
    {
        offset.y = Mathf.SmoothDampAngle(
            offset.y,
            dipTarget,
            ref dipSmoothVel,
            cameraDipTime
            );
    }
    #endregion
}