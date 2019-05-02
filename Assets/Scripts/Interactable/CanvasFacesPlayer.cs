using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CanvasFacesPlayer : MonoBehaviour
{
    private RectTransform rTransform;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
        rTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        rTransform.LookAt(-mainCam.transform.position);
    }
}
