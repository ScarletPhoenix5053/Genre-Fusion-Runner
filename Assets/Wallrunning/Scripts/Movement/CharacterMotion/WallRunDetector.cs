using UnityEngine;
using System;

public class WallRunDetector : MonoBehaviour
{
#pragma warning disable 0414
#pragma warning disable 0649
    [SerializeField] private float wallCheckDistance = 2f;
    [SerializeField] private LayerMask wallDetectionMask;
    [SerializeField] private float minimumNormalDeviation = 10f;
#pragma warning restore 0649
#pragma warning disable 0414

    private BoxCollider currentWall;
    private BoxCollider prevWall;

    private int wallCheckDir = 0;

    private bool drawGizmo = false;
    private const float cubeGizmoSize = 0.5f;
        
    public event NewWallDetectedHandler OnNewWallDetected;
    public event DetatchedFromWallHandler OnDetatchFromWall;

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        // Draw line to side of player
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.right * wallCheckDir * wallCheckDistance);

        if (currentWall != null)
        {
            // Draw bounds of current wall
            var box = currentWall.GetComponent<BoxCollider>().bounds;
            Gizmos.DrawWireCube(currentWall.transform.position, box.size);
        }

        drawGizmo = false;
    }
    private void FixedUpdate()
    {
    }
    
    public void DetatchFromWall()
    {
        prevWall = currentWall;
        currentWall = null;
        OnDetatchFromWall?.Invoke();
    }
    /// <summary>
    /// Re-enables use of the last touched wall
    /// </summary>
    public void RestorePrevWall()
    {
        prevWall = null;
    }

    public void CheckForWall(int direction)
    {
        drawGizmo = true;

        // Validate sign
        if (direction == 0) throw new InvalidSignException();
        direction = Math.Sign(direction);

        // Store check direction
        wallCheckDir = direction;

        // If already on a wall
        if (currentWall != null)
        {
            RaycastHit hit;
            var detected = RaycastForCollider(direction, out hit);

            // If no wall or a different wall is detected
            if (!detected || currentWall != hit.collider)
            {
                // Detatch
                DetatchFromWall();
            }
        }
        // Else begin checking for new wall
        else
        {
            // Check for any collision to set direction
            RaycastHit hit;
            var detected = RaycastForCollider(direction, out hit);

            if (detected)
            {

                // Check if boxcollider            
                var detectedTransform = hit.transform;
                var detectedWall = hit.collider as BoxCollider;
                if (detectedWall == null)
                {
                    return;
                }

                // Check if is current wall
                if (currentWall == detectedWall)
                {
                    return;
                }

                // Check if previous wall
                if (prevWall == detectedWall)
                {
                    return;
                }

                // Check normal of collision

                // Set to current wall transform
                prevWall = currentWall;
                currentWall = detectedWall;
                OnNewWallDetected?.Invoke(currentWall, direction);
            }
        }
    }
    public Vector3 GetWallRunEulers()
    {
        if (WallIsNull()) return transform.eulerAngles;

        // Calculate desired rotation
        var playerPos = transform.position;
        var closestPointOnCol = currentWall.ClosestPoint(playerPos);
        var dirBetweenPoints = playerPos - closestPointOnCol;

        var angleToWorldForward = Vector3.Angle(Vector3.forward, dirBetweenPoints);
        if (dirBetweenPoints.x < 0) angleToWorldForward = 360 - angleToWorldForward;

        var playerAngle = transform.eulerAngles.y;
        if (playerAngle > 360) playerAngle -= 360;
        if (playerAngle < 0) playerAngle += 360;
        // Relative angle frames angle perpendicular to wall surface as 0 degrees
        var relativePlayerAngle = playerAngle - angleToWorldForward;
        if (relativePlayerAngle > 360) relativePlayerAngle -= 360;
        if (relativePlayerAngle < 0) relativePlayerAngle += 360;

        // Create desired angle based on relative player angle
        var desiredAngle = angleToWorldForward;
        if (relativePlayerAngle > 180)
        {
            desiredAngle -= 90;
        }
        else
        {
            desiredAngle += 90;
        }
        
        //Debug.DrawRay(playerPos, Quaternion.Euler(0, desiredAngle, 0) * Vector3.forward *20);
        //Debug.Log("WF, " + angleToWorldForward + " | PR, " + playerAngle + " | DR, " + desiredAngle);

        return new Vector3(0, desiredAngle, 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="wallOffset">desired spacing between closest point on collider and actor centre</param>
    /// <returns></returns>
    public Vector3 GetWallRunStartPos(float wallOffset)
    {
        if (WallIsNull()) return transform.position;

        // Init
        var playerPos = transform.position;
        var closestPointOnCol = currentWall.ClosestPoint(playerPos);
        var dirBetweenPoints = (playerPos - closestPointOnCol).normalized;

        // Calculate
        var dirToNewCentre = dirBetweenPoints * wallOffset;
        return closestPointOnCol + dirToNewCentre;
    }

    private bool WallIsNull()
    {
        // Ensure there is a wall to work with
        if (currentWall == null)
        {
            Debug.LogWarning("There is no wall active, therefore cannot provide a new rotation");
            return true;
        }

        return false;
    }
    private bool RaycastForCollider(int direction, out RaycastHit hit)
    {
        // Validate sign
        if (direction == 0) throw new InvalidSignException();
        direction = Math.Sign(direction);

        var wallcheckRay = new Ray(transform.position, transform.right * direction);
        var detected = Physics.Raycast(wallcheckRay, out hit, wallCheckDistance, wallDetectionMask);
        return detected;
    }
}
public delegate void NewWallDetectedHandler(BoxCollider newWallTransform, int newWallDirection);
public delegate void DetatchedFromWallHandler();