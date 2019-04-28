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
        motionForce = motion * forceStrength * Time.fixedDeltaTime * forceMultiplicationFactor;
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

    public virtual void Jump() => throw new System.NotImplementedException();
    public virtual void Jump(Vector2 dir) => throw new System.NotImplementedException();
    public virtual void Sprint(bool active) => throw new System.NotImplementedException();
}
