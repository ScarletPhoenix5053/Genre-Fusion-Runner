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

    #region Methods
    public void Turn(Vector2 input)
    {
        // Create rotation values
        yaw += input.x;
        pitch -= input.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // Apply rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw, currentRotation.z), ref rotationSmoothVel, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // Control target rotation
        if (ControlTargetRotation)
        {
            Vector3 e = transform.eulerAngles;
            e.x = 0;
            target.eulerAngles = e;
        }

        // Move position
        transform.position = (target.position - transform.forward * distFromTarget) + offset;
    }    

    public void SetTilt(float angle)
    {
        // potential cover up for weird over-tilting: clamp between 0 and +-10 depending on last angle entered.
        //Debug.Log("tilting to " + angle);
        targetAngle = angle;

        if (tiltRoutine != null) StopCoroutine(tiltRoutine);
        tiltRoutine = TiltCameraRoutine();
        StartCoroutine(tiltRoutine);

        //transform.eulerAngles = currentRotation;
    }

    private IEnumerator TiltCameraRoutine()
    {
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
        }
    }
    #endregion
}