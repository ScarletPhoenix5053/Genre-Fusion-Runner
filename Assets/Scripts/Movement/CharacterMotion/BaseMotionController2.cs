using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerRefs2))]
[RequireComponent(typeof(CoalescingForce))]
public abstract class BaseMotionController2 : MonoBehaviour
{
    protected virtual void Awake()
    {
        refs = GetComponent<PlayerRefs2>();
        cf = refs.CoalescingForce;
    }
    
    protected PlayerRefs2 refs;
    protected CoalescingForce cf;

    protected const float forceMultiplicationFactor = 10000f;
    protected Vector3 ToForceInstant(Vector3 force) => force * forceMultiplicationFactor;
    protected Vector3 ToForceOverFixedTime(Vector3 force) => force * Time.fixedDeltaTime * forceMultiplicationFactor;

    protected Vector2 input;
    protected Vector3 motion = Vector3.zero;
    protected Vector3 motionForce = Vector3.zero;

    public virtual float Speed => cf.Speed;
    
    public virtual void MoveHorizontal(Vector2 input)
    {
        this.input = input;
    }
    protected virtual void CreateForcesByInput()
    {
        MapInputToMotion();
        CreateMotionForce(0f);
    }
    protected virtual void MapInputToMotion()
    {
        motion = new Vector3(this.input.x, 0, this.input.y);
    }
    protected virtual void CreateMotionForce(float forceStrength)
    {
        motionForce = ToForceOverFixedTime(motion * forceStrength);
    }
    protected virtual void LimitMotionForce(float to)
    {
        motionForce = Vector3.ClampMagnitude(motionForce, to * forceMultiplicationFactor);
    }
    protected virtual void ApplyHorizontalMotionForce()
    {
        motionForce = transform.TransformDirection(motionForce);
        cf.AddForce(motionForce);
    }

    [Header("Sprint")]
    [SerializeField] private float sprintForce;
    protected bool sprint = false;

    public virtual void Sprint(bool active) => sprint = active;

    protected virtual void AddSprintForce()
    {
        if (!sprint) return;
        motionForce += ToForceOverFixedTime(Vector3.forward * sprintForce);
    }

    #region Jump
    #region Inspector
    [Header("Jump")]
    [SerializeField] protected float jumpForce = 12f;
    [SerializeField] private float lateralJumpForce = 2f;
    #endregion
    public virtual void Jump()
    {
        cf.AddForce(ToForceInstant(Vector3.up * jumpForce));
    }
    public virtual void Jump(Vector2 dir)
    {
        Jump();

        var jumpForce = ToForceInstant(dir.Flatten().normalized * lateralJumpForce);
        jumpForce = transform.TransformDirection(jumpForce);
        cf.AddForce(jumpForce);
    }
    #endregion

}
