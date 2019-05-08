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

    private void Update()
    {
        SmoothTowardsTargetTilt();
    }
    public void Turn(Vector2 input, float lagTime = 0f)
    {
        // Create rotation values
        yaw += input.x;
        pitch -= input.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // Apply rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw, currentRotation.z), ref rotationSmoothVel, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // Control target rotation
        this.lagTime = lagTime;
        SmoothTargetRotation();

        // Move position
        transform.position = (target.position - transform.forward * distFromTarget) + offset;
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
    public void SetTilt(float angle)
    {
        targetTilt = angle;
    }
    private float targetTilt = 0f;
    private float smoothVel = 0f;
    private void SmoothTowardsTargetTilt()
    {
        currentRotation.z = Mathf.SmoothDampAngle(
            currentRotation.z,
            targetTilt,
            ref smoothVel,
            tiltSmoothTime
            );
    }

    #endregion
    #region Camera Dip
    [SerializeField] private float cameraDip = 1f;
    private bool dipped = false;
    public void DipCamera(bool dip)
    {
        if (dip && !dipped)
        {
            offset.y -= cameraDip;
            dipped = true;
        }
        else if (dipped)
        {
            offset.y += cameraDip;
            dipped = false;
        }
    }
    #endregion
}