using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    [SerializeField] private float launchForce = 100;
    [SerializeField] [Tooltip("Local Vector")] private Vector3 laucnhDirection = Vector3.up;

    private void OnTriggerEnter(Collider other)
    {
        CoalescingForce cf;
        GroundChecker groundChecker;
        if (cf = other.GetComponent<CoalescingForce>())
        {
            // Lift object over check radius if it has a ground checker
            if (groundChecker = other.GetComponent<GroundChecker>())
            {
                other.transform.position += Vector3.up * (groundChecker.CheckRadius + 0.1f);
            }

            // Add launch force
            cf.AddForce(ToForce.Instant(transform.up * launchForce));
        }
    }
}
