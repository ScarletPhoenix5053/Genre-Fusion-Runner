using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KanaPickup : MonoBehaviour
{
    public bool Active = true;

    private Kana kana;
    private Text display;
    private GameObject particles;
    [Header("Effects")]
    [SerializeField] private new GameObject light;
    [SerializeField] private new AudioSource audio;
    [SerializeField] private float pitchVariance = .2f;
    private float pitchOnAwake;
    [Header("Player Detection")]
#pragma warning disable
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float checkRadius;
    [SerializeField] private Vector3 checkOffset;
#pragma warning restore

    public void SetKana(Kana to)
    {
        kana = to;
        display.text = kana.Character.ToString();
        SetActive(true);
    }

    private void Awake()
    {
        display = GetComponentInChildren<Text>();
        particles = GetComponentInChildren<ParticleSystem>().gameObject;
        pitchOnAwake = audio.pitch;

        if (kana == null)
        {
            SetActive(false);
            return;
        }
    }
    private void LateUpdate()
    {
        if (!Active) return;
        
        var playerIsNear = Physics.OverlapSphere(transform.position + checkOffset, checkRadius, playerMask);
        if (playerIsNear.Length > 0)
        {
            Activate();
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Active ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position + checkOffset, checkRadius);
    }
    
    public void SetActive(bool active)
    {
        Active = active;
        display.enabled = active;
        particles.SetActive(active);
        light.SetActive(active);
    }
    public void Activate()
    {
        if (kana != null)
            HaikuCollectionSystem.Instance.CollectKana(kana);

        audio.pitch = Random.Range(pitchOnAwake - pitchVariance, pitchOnAwake + pitchVariance);
        audio.Play();
        SetActive(false);
    }
}
