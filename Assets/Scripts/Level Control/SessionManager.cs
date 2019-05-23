using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance;
    private void ValidateSingleton()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Destroy(fader.gameObject);
            Destroy(gameObject);
        }
    }

    [Header("Databases")]
    [SerializeField] private HaikuDatabase HaikuDatabase;

    [SerializeField] private float stageChangeDelay = 2f;
    [SerializeField] private float fadeToWhiteTime = 0.2f;
    [SerializeField] private float fadeToLevelTime = 1f;
    [SerializeField] private UIImageFader fader;
    private IEnumerator stageChangeCountdownRoutine;

    public Haiku ActiveHaiku { get; private set; }
    public Haiku PrevHaiku { get; private set; }

    public static event NewStageHandler OnStageChange;

    private const string playerTag = "Player";
    private Transform playerTransform;
    private Vector3 playerRotationOnLoad;
    private Vector3 playerPositionOnLoad;

    private void Awake()
    {
        ValidateSingleton();
        if (Instance != this) return;
        Debug.Log("Running awake");
        

        // Don't destroy on load
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(fader.gameObject);
        DontDestroyOnLoad(fader.transform.parent.gameObject);

        // Init
        fader.Transparency = 0f;
        PrevHaiku = HaikuDatabase.Haiku[0];
        ActiveHaiku = HaikuDatabase.Haiku[0];
        GetPlayerTransform();

        // Events
        OnStageChange += LoadNewScene;
        SceneManager.sceneLoaded -= AlignPlayerPosInNewScene;
        SceneManager.sceneLoaded += AlignPlayerPosInNewScene;
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

        // Update Haiku
        PrevHaiku = ActiveHaiku;
        do
        {
            ActiveHaiku = HaikuDatabase.GetRandomHaiku();
        }
        while (ActiveHaiku.Name == PrevHaiku.Name);

        Debug.Log("Next haiku is " + ActiveHaiku.Name);
    }
    public void LoadNewScene()
    {
        // Store player pos
        GetPlayerTransform();
        playerPositionOnLoad = playerTransform.position;
        playerRotationOnLoad = playerTransform.eulerAngles;
        Debug.Log("Storing " + playerRotationOnLoad);

        // Load
        Debug.Assert(ActiveHaiku != null);
        Debug.Assert(ActiveHaiku.Scene != null);
        SceneManager.LoadScene(ActiveHaiku.Scene);

        
    }
    private void AlignPlayerPosInNewScene(Scene activeScene, LoadSceneMode loadSceneMode) => AlignPlayerPosInNewScene();
    public void AlignPlayerPosInNewScene()
    {
        Debug.Log("Loading" + playerRotationOnLoad);
        GetPlayerTransform();
        FindObjectOfType<CameraController>().AdjustTarget(playerRotationOnLoad);
        playerTransform.position = playerPositionOnLoad;
        playerTransform.eulerAngles = playerRotationOnLoad;
    }

    private void GetPlayerTransform() => playerTransform = GameObject.FindGameObjectWithTag(playerTag).transform;

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
        yield return new WaitForSeconds(stageChangeDelay);
        OnStageChange?.Invoke();
        stageChangeCountdownRoutine = null;
    }
}
public delegate void NewStageHandler();