﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KanaPickup : MonoBehaviour
{
    public bool Active = true;

    private Kana kana;
    private Text display;
    private GameObject particles;
    [Header("Player Detection")]
#pragma warning disable
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float checkRadius;
    [SerializeField] private Vector3 checkOffset;
#pragma warning restore

    public void SetKana(Kana to)
    {
        kana = to;
        SetActive(true);
        display.text = kana.Character.ToString();
    }

    private void Awake()
    {
        display = GetComponentInChildren<Text>();
        particles = GetComponentInChildren<ParticleSystem>().gameObject;

        if (kana == null)
        {
            SetActive(false);
            return;
        }
    }
    private void Update()
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
    }
    public void Activate()
    {
        if (kana != null)
        HaikuCollectionSystem.Instance.CollectKana(kana);
        SetActive(false);
    }
}
