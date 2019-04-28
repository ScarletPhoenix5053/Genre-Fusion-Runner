using UnityEngine;
using System.Collections.Generic;

public class PlayerMotionControl
{
    public readonly AcceleratingCharacterMotionController Normal;
    public readonly WallrunMotionController Wallrun;
    public readonly SlideMotionController Slide;

    // EXPAND TO ALLOW THIS CLASS TO HANDLE JUGGLE MOTION CONTROLLERS AND PLAYER INPUT AS INSTRUCTED BY MAIN CONTROLLER CLASS
    private IPlayerInput playerInput;
    private List<BaseMotionController> allMotionControllers = new List<BaseMotionController>();

    public void SetActiveMotionController(CharacterState to)
    {
        ActiveMotionControllerId = to;
        foreach (BaseMotionController mc in allMotionControllers)
        {
            if (mc == ActiveMotionController) mc.enabled = true;
            else mc.enabled = false;
        }
    }
    public CharacterState ActiveMotionControllerId { get; private set; }
    public BaseMotionController ActiveMotionController => allMotionControllers[(int)ActiveMotionControllerId];

    /// <summary>
    /// 1: Accel
    /// 2: Wallrun
    /// 3: Slide
    /// </summary>
    /// <param name="motionControllers"></param>
    public PlayerMotionControl(IPlayerInput input, BaseMotionController[] motionControllers)
    {
        // Assign
        Normal = motionControllers[0] as AcceleratingCharacterMotionController;
        Wallrun = motionControllers[1] as WallrunMotionController;
        Slide = motionControllers[2] as SlideMotionController;
        playerInput = input;

        // Store in collection
        allMotionControllers.AddRange(
            new BaseMotionController[3]
            {
                Normal,
                Wallrun,
                Slide,
            });

        // Set default
        SetActiveMotionController(CharacterState.Normal);
    }
}
public class PlayerMotionControl2
{
    public readonly GroundedMotionController Normal;
    public readonly SlideMotionController2 Slide;

    // EXPAND TO ALLOW THIS CLASS TO HANDLE JUGGLE MOTION CONTROLLERS AND PLAYER INPUT AS INSTRUCTED BY MAIN CONTROLLER CLASS
    private IPlayerInput playerInput;
    private List<BaseMotionController2> allMotionControllers = new List<BaseMotionController2>();

    public void SetActiveMotionController(CharacterState to)
    {
        ActiveMotionControllerId = to;
        foreach (BaseMotionController2 mc in allMotionControllers)
        {
            if (mc == ActiveMotionController) mc.enabled = true;
            else mc.enabled = false;
        }
    }
    /*
    public void SetActiveMotionController(CharacterState to)
    {
        ActiveMotionControllerId = to;
        foreach (BaseMotionController2 mc in allMotionControllers)
        {
            if (mc == ActiveMotionController) mc.enabled = true;
            else mc.enabled = false;
        }
    }
    */
    public CharacterState ActiveMotionControllerId { get; private set; } = CharacterState.Normal;
    public BaseMotionController2 ActiveMotionController => allMotionControllers[(int)ActiveMotionControllerId];

    /// <summary>
    /// 1: Accel
    /// 2: Wallrun
    /// 3: Slide
    /// </summary>
    /// <param name="motionControllers"></param>
    public PlayerMotionControl2(IPlayerInput input, BaseMotionController2[] motionControllers)
    {
        // Assign
        Normal = motionControllers[0] as GroundedMotionController;
        //Wallrun = motionControllers[1] as WallrunMotionController;
        Slide = motionControllers[1] as SlideMotionController2;
        playerInput = input;

        // Store in collection
        allMotionControllers.Add(Normal);
        allMotionControllers.AddRange(
            new BaseMotionController2[2]
            {
                Normal,
                Slide
            });

        // Set default
        SetActiveMotionController(CharacterState.Normal);
    }
}
