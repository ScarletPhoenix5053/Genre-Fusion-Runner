using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("General")]
    [SerializeField] private float actorHeight = 2f;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private Transform transformOverride;
    [SerializeField] private Collider mainCollider;
    [Header("Smoothing")]
    [SerializeField] private bool smooth;
    [SerializeField] private float smoothSpeed;
    [Header("Check Settings")]
    [SerializeField] private Vector3 castOrigin = new Vector3(0, 1.2f, 0);
    [SerializeField] private Vector3 surfaceCheckPoint = new Vector3(0, -0.87f, 0);
    [SerializeField] private float surfaceSphereCastRadius = 0.17f;
    [SerializeField] private float surfaceSphereCastDist = 20f;
    [SerializeField] private float surfaceCheckRadius = 0.57f;
#pragma warning restore 0649
    #endregion
    #region Private Vars
    private RaycastHit groundHit;
    #endregion
    #region Properties
    public bool Grounded
    {
        get
        {
            CheckGrounding();
            return grounded;
        }
    }
    private bool grounded  = false;

    public float CheckRadius => surfaceCheckRadius;

    protected Vector3 CastDirection
    {
        get
        {
            var a = MainTransform.TransformPoint(castOrigin);
            var b = MainTransform.TransformPoint(surfaceCheckPoint);
            return b - a;
        }
    }
    protected Transform MainTransform => transformOverride == null ? transform : transformOverride;
    #endregion

    #region Unity Messages
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(MainTransform.TransformPoint(surfaceCheckPoint), surfaceSphereCastRadius);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawRay(MainTransform.TransformPoint(castOrigin), CastDirection.normalized * surfaceSphereCastDist);
        Gizmos.DrawWireSphere(MainTransform.TransformPoint(surfaceCheckPoint), surfaceCheckRadius);
    }
    private void Update()
    {
        CheckGrounding();
    }
    #endregion

    #region Methods
    public void CheckGrounding()
    {
        SurfaceCheck();
    }
    private void SurfaceCheck()
    {
        // Generate ray
        var ray = 
            new Ray(
                MainTransform.TransformPoint(castOrigin),
                CastDirection);
        RaycastHit tempHit = new RaycastHit();

        // Cast initial ray
        if (Physics.SphereCast(ray, surfaceSphereCastRadius, out tempHit, surfaceSphereCastDist, surfaceMask))
        {
            // Confirm hit ground
            SurfaceConfirm(tempHit);
        }
        else
        {
            grounded = false;
        }
    }
    private void SurfaceConfirm(RaycastHit tempHit)
    {
        // Check area around grounding point
        Collider[] colBuffer = new Collider[3];
        int num = 
            Physics.OverlapSphereNonAlloc(
                MainTransform.TransformPoint(surfaceCheckPoint),
                surfaceCheckRadius,
                colBuffer,
                surfaceMask);

        grounded = false;

        // validate grounding
        for (int i = 0; i < num; i++)
        {
            // If valid
            if (colBuffer[i].transform == tempHit.transform)
            {
                groundHit = tempHit;
                grounded = true;

                // Snap to surface
                if (!smooth)
                {
                    MainTransform.position =
                        new Vector3(
                            MainTransform.position.x,
                            groundHit.point.y + actorHeight / 2,
                            MainTransform.position.z
                            );
                }
                else
                {
                    MainTransform.position = Vector3.Lerp(
                        MainTransform.position,
                        new Vector3(
                            MainTransform.position.x,
                            groundHit.point.y + actorHeight / 2,
                            MainTransform.position.z
                            ),
                        smoothSpeed * Time.deltaTime
                        );
                }
                break;
            }
        }
        /*
        // Extra check to prevent player from sticking
        if (num <= 1 && tempHit.distance <= 3.1f)
        {
            if (colBuffer[0] != null)
            {
                Ray ray = new Ray(MainTransform.TransformPoint(castOrigin), CastDirection);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 3.1f, surfaceMask))
                {
                    if (hit.transform != colBuffer[0].transform)
                    {
                        connected = false;
                        return;
                    }
                }
            }
        }*/ 
    }
    #endregion
}
