using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[RequireComponent(typeof(Image))]
public class UIImageFader : MonoBehaviour
{
    [SerializeField][Range(0,1)] private float fadeAmount;
    private Image image;
    private IEnumerator activeRoutine;

    public event FadeEndHandler OnFadeEnd;

    public float Transparency
    {
        get => fadeAmount;
        set
        {
            fadeAmount = value;

            image.color = new Color(
                image.color.r,
                image.color.g,
                image.color.b,
                fadeAmount
                );
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void FadeOverSeconds(float time, bool active)
    {
        var endT = active ? 1 : 0;
        var startT = active ? 0 : 1;

        if (activeRoutine != null) activeRoutine = null;
        activeRoutine = FadeOverTimeRoutine(time, startT, endT);
        StartCoroutine(activeRoutine);
    }

    private IEnumerator FadeOverTimeRoutine(float time, float startTransparency, float endTransparency)
    {
        Transparency = startTransparency;

        var timer = 0f;
        do
        {
            Transparency = Mathf.Lerp(startTransparency, endTransparency, timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (timer < time);

        Transparency = endTransparency;
        OnFadeEnd?.Invoke();
    }
}
public delegate void FadeEndHandler();