using UnityEngine;

namespace AJTECH.Charcont
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector
#pragma warning disable 0649
        [SerializeField] private Transform target;
        [SerializeField] private float distFromTarget = 8.5f;
        [SerializeField] private Vector2 pitchMinMax = new Vector2(-48, 85);
        [SerializeField] private float rotationSmoothTime = 0.12f;
#pragma warning restore 0649
        #endregion
        #region Private Vars
        private Vector3 rotationSmoothVel;
        private Vector3 currentRotation;

        private float yaw;
        private float pitch;
        #endregion

        #region Methods
        public void Turn(Vector2 input)
        {
            // Create rotation values
            yaw += input.x;
            pitch -= input.y;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

            // Apply rotation
            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVel, rotationSmoothTime);
            transform.eulerAngles = currentRotation;

            // Control target rotation
            Vector3 e = transform.eulerAngles;
            e.x = 0;
            target.eulerAngles = e;

            // Move position
            transform.position = target.position - transform.forward * distFromTarget;
        }
        #endregion
    }
}