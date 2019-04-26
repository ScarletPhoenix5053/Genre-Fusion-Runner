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
