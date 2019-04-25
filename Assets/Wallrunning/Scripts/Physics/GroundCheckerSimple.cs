using UnityEngine;

public class GroundCheckerSimple : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [Header("General")]
    [SerializeField] private float actorHeight = 2f;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private Collider mainCollider;
    [Header("Check Settings")]
    [SerializeField] private Vector3 castOrigin = new Vector3(0, 1.2f, 0);
    [SerializeField] private Vector3 surfaceCheckPoint = new Vector3(0, -0.87f, 0);
    [SerializeField] private float surfaceSphereCastRadius = 0.17f;
    [SerializeField] private float surfaceSphereCastDist = 20f;
    [SerializeField] private float surfaceCheckRadius = 0.57f;
#pragma warning restore 0649
    #endregion
    #region Private Vars
    private bool grounded = false;
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

    protected Vector3 CastDirection
    {
        get
        {
            var a = transform.TransformPoint(castOrigin);
            var b = transform.TransformPoint(surfaceCheckPoint);
            return b - a;
        }
    }
    #endregion

    #region UnityRuntime
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.TransformPoint(surfaceCheckPoint), surfaceSphereCastRadius);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.TransformPoint(castOrigin), CastDirection.normalized * surfaceSphereCastDist);
        Gizmos.DrawWireSphere(transform.TransformPoint(surfaceCheckPoint), surfaceCheckRadius);
    }
    private void FixedUpdate()
    {
        CheckGrounding();
    }
    #endregion

    #region Methods
    private void CheckGrounding()
    {
        // Generate ray
        var ray =
            new Ray(
                transform.TransformPoint(castOrigin),
                CastDirection);
        RaycastHit tempHit = new RaycastHit();

        // Cast initial ray
        if (Physics.SphereCast(ray, surfaceSphereCastRadius, out tempHit, surfaceSphereCastDist, surfaceMask))
        {
            // Confirm hit ground
            GroundConfirm(tempHit);
        }
        else
        {
            grounded = false;
        }
    }
    private void GroundConfirm(RaycastHit tempHit)
    {
        // Check area around grounding point
        Collider[] colBuffer = new Collider[3];
        int num =
            Physics.OverlapSphereNonAlloc(
                transform.TransformPoint(surfaceCheckPoint),
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
            }
        }
    }
    #endregion
}