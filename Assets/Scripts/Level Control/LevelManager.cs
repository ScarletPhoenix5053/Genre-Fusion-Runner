using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private void ValidateSingleton()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) SafeDestroy.Object(this);
    }

    [SerializeField] private float stageChangeDelay = 2f;
    [SerializeField] private float fadeToWhiteTime = 0.2f;
    [SerializeField] private float fadeToLevelTime = 1f;
    [SerializeField] private UIImageFader fader;
    private IEnumerator stageChangeCountdownRoutine;

    public static event NewStageHandler OnStageChange;

    private void Awake()
    {
        ValidateSingleton();
        fader.Transparency = 0f;        
    }
    private void OnValidate()
    {
        if (stageChangeDelay < 0) stageChangeDelay = 0;
    }

    public void BeginStageChange()
    {
        if (stageChangeCountdownRoutine != null) return;

        stageChangeCountdownRoutine = StageCountdownRoutine();
        StartCoroutine(stageChangeCountdownRoutine);
        Invoke("FadeToWhite", stageChangeDelay - fadeToWhiteTime);
    }

    private void FadeToWhite()
    {
        fader.OnFadeEnd += FadeToLevel;
        fader.FadeOverSeconds(fadeToWhiteTime, active: true);
    }
    private void FadeToLevel()
    {
        fader.OnFadeEnd -= FadeToLevel;
        fader.FadeOverSeconds(fadeToLevelTime, active: false);
    }

    private IEnumerator StageCountdownRoutine()
    {
        Debug.Log("Waiting for stage change");

        yield return new WaitForSeconds(stageChangeDelay);
        OnStageChange?.Invoke();
        stageChangeCountdownRoutine = null;

        Debug.Log("Changing stage");
    }
}
public delegate void NewStageHandler();