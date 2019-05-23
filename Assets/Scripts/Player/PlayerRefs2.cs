using UnityEngine;
using System.Collections;

public class PlayerRefs2 : MonoBehaviour
{
    [Header("Camera")]
    public CameraController Cam;
    [Header("Audio")]
    public PlayerAudio PlayerAudio;
    [Header("Motion Controllers")]
    public CoalescingForce CoalescingForce;
    [Header("Physics and Motion")]
    public GroundChecker GroundChecker;
    public CapsuleCollider MainCollider;
    public TrueDrag Drag;
    [Header("Prefrences")]
    public PlayerPreferenceGroup PlayerPrefrences;
    
}
