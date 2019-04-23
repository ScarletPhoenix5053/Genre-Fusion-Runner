using UnityEngine;
using System.Collections;

namespace AJTECH.Charcont
{
    /******************************************************************************
     * 
     * Original Character controller by AJTech @
     * https://www.youtube.com/watch?v=ona6VvuWLkI 
     * 
     ******************************************************************************/
    public class PlayerController : MonoBehaviour
    {
#pragma warning disable 0649
#pragma warning disable 0414
        #region Inspector
        [Header("Player")]
        [SerializeField] private float playerHeight = 2f;
        [Header("Walk")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private bool smooth;
        [SerializeField] private float smoothSpeed;
        [Header("Jump")]
        [SerializeField] private float jumpForce;
        [SerializeField] private float jumpSpeed;
        [SerializeField] private float jumpDecrease;
        [Header("Phyiscs")]
        [SerializeField] private float gravity = 1.2f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private SphereCollider sphereCol;
        #endregion
        #region Private Vars
        #region Motion
        private Vector3 velocity;
        private Vector3 move;
        private const string horizontalAxis = "Horizontal";
        private const string verticalAxis = "Vertical";

        private bool inputJump = false;
        private float jumpHeight = 0;
        private const KeyCode jumpKey = KeyCode.Space;
        #endregion
        #region Gravity
        private const float groundSphereCastRadius = 0.17f;
        private const float groundSphereCastDist = 20f;
        private const float groundCheckRadius = 0.57f;
        private bool grounded;
        private float curentGravity = 0;
        private Vector3 liftPoint = new Vector3(0, 1.2f, 0);
        private Vector3 groundCheckPoint = new Vector3(0, -0.87f, 0);
        private RaycastHit groundHit;
        #endregion
        #endregion
#pragma warning restore 0649
#pragma warning restore 0414
        private void Update()
        {
            Gravity();              // Moved to new gravity components
            SimpleMove();           // Moved to player controller (interaction w/ translator)
            Jump();                 
            FinalMove();            // Moved to new translator component
            GroundCheck();          // Moved to new surface cheker component
            CollisionCheck();       // Moved to new true collider component
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckPoint), groundCheckRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.TransformPoint(groundCheckPoint));
            Gizmos.DrawWireSphere(transform.TransformPoint(groundCheckPoint), groundSphereCastRadius);
        }

        #region Methods
        #region Movement
        private void SimpleMove()
        {
            move = new Vector3(Input.GetAxis(horizontalAxis), 0, Input.GetAxis(verticalAxis));
            velocity += move;
        }
        private void FinalMove()
        {
            Vector3 vel = new Vector3(velocity.x, velocity.y, velocity.z) * movementSpeed;
            //velocity = new Vector3(move.x, 0, move.z) * movementSpeed;
            //velocity += Vector3.up * jumpHeight;
            //velocity += Vector3.down * curentGravity;

            vel = transform.TransformDirection(vel);
            transform.position += vel * Time.deltaTime;

            velocity = Vector3.zero;
        }
        private void Jump()
        {
            // Check for roof
            bool canJump = false;
            canJump = !Physics.Raycast(new Ray(transform.position, Vector3.up), playerHeight, groundMask);

            if (grounded && jumpHeight > 0.2f || jumpHeight <= 0.2f && grounded)
            {
                jumpHeight = 0;
                inputJump = false;
            }

            if (grounded && canJump)
            {
                // Give initial boost to overcome grounding sphere
                if (Input.GetKey(jumpKey))
                {
                    inputJump = true;
                    transform.position += Vector3.up * 0.6f * 2;
                    jumpHeight += jumpForce;
                }
            }
            else
            {
                // Decay jumpheight
                if (!grounded)
                {
                    jumpHeight -= jumpHeight * jumpDecrease * Time.deltaTime;
                }
            }

            velocity.y += jumpHeight;
        }
        #endregion
        #region Gravity
        private void Gravity()
        {
            if (!grounded)
            {
                velocity.y -= gravity;
            }
            else
            {
                curentGravity = 0;
            }
        }
        private void GroundCheck()
        {
            var ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
            RaycastHit tempHit = new RaycastHit();

            // Cast initial ray
            if (Physics.SphereCast(ray, groundSphereCastRadius, out tempHit, groundSphereCastDist, groundMask))
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
            //float currentSlope = Vector3.Angle(tempHit.normal, Vector3.up);

            Collider[] colBuffer = new Collider[3];
            int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint), groundCheckRadius, colBuffer, groundMask);

            grounded = false;

            // validate grounding
            for (int i = 0; i < num; i++)
            {
                // If valid
                if (colBuffer[i].transform == tempHit.transform)
                {
                    groundHit = tempHit;
                    grounded = true;

                    // Snap to ground
                    if (!inputJump)
                    {
                        if (!smooth)
                        {
                            transform.position =
                                new Vector3(
                                    transform.position.x,
                                    groundHit.point.y + playerHeight / 2,
                                    transform.position.z
                                    );
                        }
                        else
                        {
                            transform.position = Vector3.Lerp(
                                transform.position,
                                new Vector3(
                                    transform.position.x,
                                    groundHit.point.y + playerHeight / 2,
                                    transform.position.z
                                    ),
                                smoothSpeed * Time.deltaTime
                                );
                        }
                    }
                    break;
                }
            }

            // Extra check to prevent player from sticking
            if (num <= 1 && tempHit.distance <= 3.1f && !inputJump)
            {
                if (colBuffer[0] != null)
                {
                    Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 3.1f, groundMask))
                    {
                        if (hit.transform != colBuffer[0].transform)
                        {
                            grounded = false;
                            return;
                        }
                    }
                }
            }
        }
        #endregion
        #region Collision
        private void CollisionCheck()
        {
            // Check for nearby coliders
            Collider[] overlaps = new Collider[4];
            int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphereCol.center), sphereCol.radius, overlaps, groundMask, QueryTriggerInteraction.UseGlobal);

            for (int i = 0; i < num; i++)
            {
                Transform t = overlaps[i].transform;
                Vector3 dir;
                float dist;

                // Check for penetration of other collider
                if (Physics.ComputePenetration(sphereCol, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
                {
                    // Generate inverse penetration and slide vel
                    Vector3 penetrationVector = dir * dist;

                    // Apply vars
                    transform.position = transform.position + penetrationVector;
                }
            }
        }
        #endregion
        #endregion
    }
}