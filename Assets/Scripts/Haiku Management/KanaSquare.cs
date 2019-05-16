using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class KanaSquare : MonoBehaviour
{
    /*
    [SerializeField] private bool clearTextOnAwake;
    private Text display;
    private void Awake()
    {
        display = GetComponent<Text>();
        if (clearTextOnAwake) display.text = ""; 
    }

    private Kana kana;
    public void SetKana(Kana to)
    {
        kana = to;
        display.text = kana.ToString();
    }
    public void ResetKana()
    {
        kana = default;
        display.text = "";
    }
    */
}
