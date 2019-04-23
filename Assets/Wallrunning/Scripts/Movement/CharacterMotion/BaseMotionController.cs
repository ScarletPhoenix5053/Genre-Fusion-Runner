using UnityEngine;
using System.Collections;

public abstract class BaseMotionController : MonoBehaviour
{
    [SerializeField] protected Translator translator;

    protected Vector3 moveVector = Vector3.zero;

    public abstract float Speed { get; }

    public abstract void MoveHorizontal(Vector2 input);
    public abstract void Jump();
    public abstract void Jump(Vector2 dir);
    public abstract void Sprint(bool active);

    protected void ApplyAndResetMotion()
    {
        // Apply and reset
        translator.AddVelocity(moveVector);
        moveVector = Vector3.zero;
    }
}
