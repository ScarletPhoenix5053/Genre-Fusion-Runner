using UnityEngine;
using System.Collections.Generic;

public class LEGACY_WindEffect : MonoBehaviour
{
    [SerializeField] private float strength = 100f;
    [SerializeField] private Vector3 direction = Vector3.left;

    public float Strength { get => strength; set { strength = value; } }
    public Vector3 Direction{ get => direction; set { direction = value; } }

    private List<CoalescingForce> allDynamicObjects = new List<CoalescingForce>();

    private void Start()
    {
        allDynamicObjects.AddRange(FindObjectsOfType<CoalescingForce>());
    }
    private void FixedUpdate()
    {
        ApplyWind();
    }

    private void ApplyWind()
    {
        foreach (CoalescingForce dynamicObject in allDynamicObjects)
        {
            //Debug.Log("Applying wind to " + dynamicObject.gameObject.name);
            dynamicObject.AddForce(direction * strength * Time.fixedDeltaTime);
        }
    }
}