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
        if (wallCheckDir == 0)
        {
            Gizmos.DrawRay(transform.position, transform.right * -1 * wallCheckDistance);
            Gizmos.DrawRay(transform.position, transform.right * 1 * wallCheckDistance);
        }
        else Gizmos.DrawRay(transform.position, transform.right * wallCheckDir * wallCheckDistance);

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

    /// <summary>
    /// Checks for and evaluates walls in both directions.
    /// Attatches to the closest one if not already on a wall.
    /// </summary>
    public void CheckForWall()
    {
        drawGizmo = true;
        if (currentWall != null) return;        

        // Check left
        wallCheckDir = -1;
        var wallLeft = CheckForNewWall();

        // Check right
        wallCheckDir = 1;
        var wallRight = CheckForNewWall();

        // Evaluate walls if two are found
        if (wallLeft != null && wallRight != null)
        {
            throw new NotImplementedException("Method CheckForWall() has not implimented resolution for finding two walls");
        }
        // Attatch to wall if only one found
        else if (wallLeft != null)
        {
            wallCheckDir = -1;
            SetCurrentWallTo(wallLeft);
        }
        else if (wallRight != null)
        {
            wallCheckDir = 1;
            SetCurrentWallTo(wallRight);
        }
        // Return if none are found
        else
        {
            wallCheckDir = 0;
            return;
        }


    }
    /// <summary>
    /// Checks for a wall in the specified direction. Throws exception if sign is 0.
    /// </summary>
    /// <param name="direction"></param>
    public void CheckForWall(int direction)
    {
        drawGizmo = true;
        BoxCollider newWall = null;

        // Validate sign
        if (SignIsNotValid(direction)) throw new InvalidSignException();
        else wallCheckDir = Math.Sign(direction);

        // If not on a wall
        if (currentWall == null)
        {
            // Check for new wall
            newWall = CheckForNewWall();
            if (newWall != null) SetCurrentWallTo(newWall);
        }
        else
        {
            CheckForNewWallOnWallEnd();
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

    private bool SignIsNotValid(int sign) => sign == 0;

    private void CheckForNewWallOnWallEnd()
    {
        RaycastHit hit;
        BoxCollider newWall = null;
        var detected = RaycastForCollider(wallCheckDir, out hit);

        // If no wall or a different wall is detected
        if (!detected || currentWall != hit.collider)
        {
            newWall = CheckForNewWall();
            if (newWall != null) SetCurrentWallTo(newWall);
            else DetatchFromWall();

        }
    }
    private void DetatchOnWallEnd()
    {
        RaycastHit hit;
        var detected = RaycastForCollider(wallCheckDir, out hit);

        // If no wall or a different wall is detected
        if (!detected || currentWall != hit.collider)
        {
            // Detatch
            DetatchFromWall();
        }
    }
    private BoxCollider CheckForNewWall()
    {
        // Check for any collision to set direction
        RaycastHit hit;
        var detected = RaycastForCollider(wallCheckDir, out hit);

        if (detected)
        {
            // Validate collider
            var detectedWall = hit.collider as BoxCollider;
            if (
                detectedWall == null ||
                // Ensure is not same or prev collider
                currentWall == detectedWall ||
                prevWall == detectedWall
                )
                return null;

            // Check normal of collision

            return detectedWall;
        }

        return null; // if nothing found
    }
    private void SetCurrentWallTo(BoxCollider newWall)
    {
        // Set to current wall transform
        prevWall = currentWall;
        currentWall = newWall;
        OnNewWallDetected?.Invoke(currentWall, wallCheckDir);
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