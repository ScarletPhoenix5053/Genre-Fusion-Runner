using UnityEngine;
using System.Collections;

/*
/// <summary>
/// Generates a jump vector based on player input. Requires a refrence to a ground checker component.
/// </summary>
public class Jumper : MonoBehaviour, ILauncherComponent
{
    [SerializeField] private Transform transformOverride;
    [SerializeField] private float defaultHeight = 10f;
    [SerializeField] private float groundCheckRadius = 0.57f;
    
    public Vector3 Velocity { get; private set; }
    protected Transform MainTransform => transformOverride == null ? transform : transformOverride;

    private void FixedUpdate()
    {
    }

    #region Methods
    public void Up()
    {
        Up(defaultHeight);
    }
    public void Up(float vel)
    {
        Velocity = Vector3.up * vel;
        BoostOverGroundStick();
    }
    public void Direction(Vector3 jumpVel)
    {
        Velocity += jumpVel;
    }
    
    private void BoostOverGroundStick()
    {
        MainTransform.position += Vector3.up * (groundCheckRadius + 0.1f);
    }
    #endregion
}
public interface ILauncherComponent : IReturnVelocity
{
    /// <summary>
    /// Launches the attatched object upwards
    /// </summary>
    void Up();
    /// <summary>
    /// Launches the attatched object upwards at a set velocity
    /// </summary>
    /// <param name="vel"></param>
    void Up(float vel);
    /// <summary>
    /// Launches the attatched object with the velocity passed into it
    /// </summary>
    /// <param name="jumpVel"></param>
    void Direction(Vector3 jumpVel);
}
*/