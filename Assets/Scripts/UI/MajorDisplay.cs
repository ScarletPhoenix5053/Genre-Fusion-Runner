using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class MajorDisplay : MonoBehaviour
{
    [SerializeField] private Text kana;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        HaikuCollectionSystem.OnCompletion += AnnounceHaikuCompletion;
    }

    public void Show(Kana kana)
    {
        this.kana.text = kana.Character.ToString();
        anim.SetTrigger("Small");
    }
    public void Show(Haiku haiku)
    {
        throw new System.NotImplementedException();
    }
    public void AnnounceHaikuCompletion(Haiku oldHaiku)
    {
        anim.SetTrigger("Large");
    }
}