using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Landing")]
    [SerializeField] private AudioSource langingSource;
    [Header("Footsteps")]
    [SerializeField] private AudioSource footStepSource;
    [SerializeField] private float distancePerStep = 1f;
    [SerializeField] private AudioClip[] footstepsStone;
    [SerializeField] private AudioClip[] footstepsWater;
    [Header("Water check")]
    [SerializeField] private float waterCheckDistance = 0.1f;
    [SerializeField] private LayerMask waterCheckMask;

    public bool TouchingWater = false;
    
    private ParkourPlayerController2 player;
    public LandingClips LandingClips;

    private float distToNextStep = 0f;

    private void Awake()
    {
        player = GetComponentInParent<ParkourPlayerController2>();
    }
    private void Update()
    {
        PlayFootsteps();
    }

    private void PlayFootsteps()
    {
        // Escape if player is not in correct state
        if (player.State != CharacterState.Normal)
        {
            if (player.State != CharacterState.Wallrun)
            {
                return;
            }
        }
        else if (!player.Grounded) return;

        // Check if standing on water
        CheckForWater();

        // Play audio based on speed
        if (distToNextStep <= 0)
        {
            PlayFootstepAudio();
        }
        distToNextStep -= player.Speed * Time.deltaTime;
    }

    private void CheckForWater()
    {
        var waterChecRay = new Ray(transform.position, Vector3.down);
        if (Physics.OverlapSphere(transform.position, waterCheckDistance, waterCheckMask).Length > 0)
        {
            TouchingWater = true;
        }
        else TouchingWater = false;
    }

    public void PlayFootstepAudio()
    {
        if (footstepsStone == null || footstepsStone.Length == 0) return;
        if (footstepsWater == null || footstepsWater.Length == 0) return;

        var randomIndex = Random.Range(0, TouchingWater ? footstepsWater.Length : footstepsStone.Length);
        footStepSource.PlayOneShot(TouchingWater ? footstepsWater[randomIndex] : footstepsStone[randomIndex]);

        distToNextStep = distancePerStep;
    }
    public void PlayLandingAudio()
    {
        CheckForWater();

        langingSource.PlayOneShot(TouchingWater ? LandingClips.Water : LandingClips.Stone);
    }
}
[System.Serializable]
public struct LandingClips
{
    public AudioClip Water;
    public AudioClip Stone;
}