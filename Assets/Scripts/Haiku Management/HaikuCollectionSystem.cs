using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCARLET.DbOverlay;

public class HaikuCollectionSystem : MonoBehaviour
{    
    #region Singleton Pattern
    public static HaikuCollectionSystem Instance { get; private set; }
    private void ValidateSingleton()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) SafeDestroy.Object(this);
    }
    #endregion

    [SerializeField] private HaikuDatabase haikuDatabase;
    [SerializeField] private KanaPickup kanaPickupObj;
    [Header("Display")]
    [SerializeField] MajorDisplay mainDisplay;
    private HaikuDisplay haikuDisplay;

    private const string idActiveHaiku = "Active Haiku: ";

    private void Awake()
    {
        ValidateSingleton();

        DebugOverlay.CreateLog(idActiveHaiku);
    }
    private void Start()
    {
        haiku = haikuDatabase.GetStartHaiku();
        DebugOverlay.UpdateLog(idActiveHaiku, haiku.Name);

        // Init Display
        haikuDisplay = GetComponent<HaikuDisplay>();
        haikuDisplay.InitDisplay(haiku);

        // Init Events
        OnCollection += UpdateHaikuDisplay;
        OnCollection += mainDisplay.Show;        
        OnCompletion += GetNewHaiku;

        // Init Kana Pickup System
        kanaPickupCollection = GameObject.FindGameObjectWithTag(kanaPickupCollectionTag).transform;
        GenerateKanaPickupsFor(haiku);
    }

    #region Collection System
    private Haiku haiku;

    public static event HaikuCollectionEventHandler OnCollection;
    public static event HaikuCompletionEventHandler OnCompletion;    

    private int KanaCollected = 0;
    public void CollectKana(Kana collectedKana)
    {
        KanaCollected++;
        OnCollection?.Invoke(collectedKana);

        if (KanaCollected >= haiku.KanaCount)
        {
            OnCompletion.Invoke(haiku);
            KanaCollected = 0;
        }
    }

    private Transform kanaPickupCollection;
    private const string kanaPickupCollectionTag = "Pickup Collection";
    
    private void GenerateKanaPickupsFor(Haiku nextHaiku)
    {
        var allKana = nextHaiku.ToKana();
        if (allKana.Length > kanaPickupCollection.childCount)
        {
            Debug.LogError("Not enough pickups for haiku: " + nextHaiku.Name);
        }
        for (int k = 0; k < allKana.Length; k++)
        {
            var kanaPickup = kanaPickupCollection.GetChild(k).GetComponent<KanaPickup>();
            kanaPickup.SetKana(to: allKana[k]);
        }
    }
    #endregion

    #region Display

    private const float displayResetDelay = 1f;
    private void GeneratePickupsAfterDelay() => Invoke("GeneratePickups", displayResetDelay + 1f);
    private void GeneratePickups() => GenerateKanaPickupsFor(haiku);
    private void InitDisplayAfterDelay() => Invoke("InitDisplay", displayResetDelay + 0.5f);
    private void InitDisplay() =>  haikuDisplay.InitDisplay(haiku);
    private void ResetDisplayAfterDelay(Haiku haiku) => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplayAfterDelay() => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplay() => haikuDisplay.ResetDisplay();
    private void GetNewHaiku(Haiku oldHaiku)
    {
        do
        {
            haiku = haikuDatabase.GetRandomHaiku();
            Debug.Log("trying to get new haiku");
        }
        while (haiku.Name == oldHaiku.Name);

        Debug.Log("new haiku is: " + haiku);
        
        DebugOverlay.UpdateLog(idActiveHaiku, haiku.Name);

        ResetDisplayAfterDelay();
        InitDisplayAfterDelay();
        GeneratePickupsAfterDelay();

        var player = FindObjectOfType<ParkourPlayerController2>();
        player.UpdateMotionProfile(haiku);
    }

    private void UpdateHaikuDisplay(Kana newKana)
    {
        Debug.Assert(newKana != null);
        haikuDisplay.ActivateKanaSquare(newKana);
    }
    #endregion    

    #region Debug
    #endregion

}
public delegate void HaikuCollectionEventHandler(Kana kana);
public delegate void HaikuCompletionEventHandler(Haiku haiku);