using UnityEngine;
using System.Collections;

public class MomentumTranslator : MonoBehaviour
{
    [Header("Momentum")]
    [SerializeField] private Vector3 drag = Vector3.one;
    [SerializeField] private Vector3 momentum = Vector3.zero;
    [SerializeField] private Vector3 momentumMaximum = new Vector3(10, 10, 10);
    
    [SerializeField] private float minimumMomentum = 0.1f;
    
    public Vector3 FrameVelocity { get; set; }
    public Vector3 Drag => drag;
    public Vector3 Momentum => momentum;

    private void Update()
    {
        DecayMomentum();
    }

    public void CommitMovement()
    {
        // MOMENTUM SYSTEM NEEDS IMPROVEMET: LOOK FOR OTHER METHODS?


        // Local vector >> World vector
        Vector3 vel = new Vector3(FrameVelocity.x, FrameVelocity.y, FrameVelocity.z);
        vel = transform.TransformDirection(vel);

        // Momentum override        
        momentum += vel;
        momentum.x = Mathf.Clamp(momentum.x, -momentumMaximum.x, momentumMaximum.x);
        momentum.y = Mathf.Clamp(momentum.y, -momentumMaximum.y, momentumMaximum.y);
        momentum.z = Mathf.Clamp(momentum.z, -momentumMaximum.z, momentumMaximum.z);

        // Apply
        transform.position += momentum * Time.fixedDeltaTime;

        // Reset FV
        FrameVelocity = Vector3.zero;
    }
    private void DecayMomentum()
    {
        DecayBySign(ref momentum.x, drag.x);
        DecayBySign(ref momentum.y, drag.y);
        DecayBySign(ref momentum.z, drag.z);
    }
    private void DecayBySign(ref float f, float decay)
    {
        // Zero-out
        if (f == 0) return;
        if (Mathf.Abs(f) < minimumMomentum)
        {
            f = 0;
            return;
        }

        // Apply decay if not zeroed
        var decayAmount = (Mathf.Sign(f) * decay) * Time.deltaTime;
        f -= decayAmount;
    }
}
public interface IHaveMomentum
{
    /// <summary>
    /// Decay of momentumover time.
    /// </summary>
    Vector3 Drag { get; }
    /// <summary>
    /// Momentum of object. Gradually decays over time.
    /// </summary>
    Vector3 Momentum { get; }
}