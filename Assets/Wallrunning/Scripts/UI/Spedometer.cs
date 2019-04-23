using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Spedometer : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private ParkourPlayerController player;
    [SerializeField] private Text display;
#pragma warning restore 0649

    private void FixedUpdate()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        display.text = "Vel: " + player.Speed.ToString();
    }
}
