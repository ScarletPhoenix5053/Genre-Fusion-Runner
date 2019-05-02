﻿using UnityEngine;
using System.Collections;

public class PlayerRefs : MonoBehaviour
{
    [Header("Camera")]
    public CameraController Cam;
    [Header("Motion Controllers")]
    public AcceleratingCharacterMotionController MotionControllerNormal;
    public WallrunMotionController MotionControllerWallrun;
    public SlideMotionController MotionControllerSlide;
    [Header("Physics and Motion")]
    public GroundChecker GroundChecker;
    public CapsuleCollider MainCollider;
    [Header("Prefrences")]
    public PlayerPreferenceGroup PlayerPrefrences;


    /*
    if (cam == null)
    {
        if (!(Camera.main != null && (cam = Camera.main.GetComponent<CameraController>())))
            Debug.LogError("Cannot find camera: unable to complete validataion.", this);
    }
    
        // Prefs
        if (preferences == null) Debug.LogError("Cannot find player prefrences: unable to complete validataion.", this);
    
        // Gravity
        if (gravityChecker == null) gravityChecker = Helper.FindRelevantComponent<GroundChecker>(transform);

        // Colliders
        if (capsuleCollider == null) capsuleCollider = Helper.FindRelevantComponent<CapsuleCollider>(transform);




    */
}