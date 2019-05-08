using UnityEngine;
using System.Collections;

public class PlayerRefs2 : MonoBehaviour
{
    [Header("Camera")]
    public CameraController Cam;
    [Header("Motion Controllers")]
    public CoalescingForce CoalescingForce;
    public GroundedMotionController GroundedMotionController;
    public SlideMotionController2 SlideMotionController;
    public WallrunMotionController2 WallrunMotionController;
    public WallClimbMotionController WallClimbMotionController;
    [Header("Physics and Motion")]
    public GroundChecker GroundChecker;
    public CapsuleCollider MainCollider;
    public TrueDrag Drag;
    [Header("Prefrences")]
    public PlayerPreferenceGroup PlayerPrefrences;
    
}
