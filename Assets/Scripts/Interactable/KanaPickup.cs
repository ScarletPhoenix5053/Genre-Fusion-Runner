using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KanaPickup : MonoBehaviour
{
    private Kana kana;
    private Text display;
    [Header("Player Detection")]
#pragma warning disable
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private float checkRadius;
    [SerializeField] private Vector3 checkOffset;
#pragma warning restore

    public void SetKana(Kana to)
    {
        kana = to;
        display.text = kana.ToString();
    }

    private void Awake()
    {
        display = GetComponentInChildren<Text>();
    }
    private void Update()
    {
        var playerIsNear = Physics.OverlapSphere(transform.position + checkOffset, checkRadius, playerMask);
        if (playerIsNear.Length > 0)
        {
            Debug.Log("Entered " + name);
            Activate();
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + checkOffset, checkRadius);
    }

    public void Activate()
    {
        HaikuCollectionSystem.Instance.CollectKana(kana);
        SafeDestroy.Object(gameObject);
    }
}
