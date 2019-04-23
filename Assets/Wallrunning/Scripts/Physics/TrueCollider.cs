using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class TrueCollider : MonoBehaviour
{
    #region Inspector
#pragma warning disable 0649
    [SerializeField] private LayerMask environmentMask;
#pragma warning restore 0649
    #endregion
    #region Private Vars
    private SphereCollider col;
    #endregion

    #region Mesasges
    private void Awake()
    {
        col = GetComponent<SphereCollider>();
    }
    private void Update()
    {
        CollisionCheck();
    }
    #endregion

    #region Methods
    private void CollisionCheck()
    {
        // Check for nearby coliders
        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(col.center), col.radius, overlaps, environmentMask, QueryTriggerInteraction.UseGlobal);

        for (int i = 0; i < num; i++)
        {
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            // Check for penetration of other collider
            if (Physics.ComputePenetration(col, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                // Generate inverse penetration and slide vel
                Vector3 penetrationVector = dir * dist;

                // Apply vars
                transform.position = transform.position + penetrationVector;
            }
        }
    }
    #endregion
}
