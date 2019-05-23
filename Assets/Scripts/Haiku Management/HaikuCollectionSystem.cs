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
    [SerializeField] private HaikuDisplay haikuDisplay;

    private const string idActiveHaiku = "Active Haiku: ";

    private void Awake()
    {
        ValidateSingleton();

        DebugOverlay.CreateLog(idActiveHaiku);
    }
    private void Start()
    {
        haiku = SessionManager.Instance.ActiveHaiku;
        DebugOverlay.UpdateLog(idActiveHaiku, haiku.Name);

        // Init Display
        haikuDisplay.InitDisplay(haiku);

        // Init Events
        OnCollection += UpdateHaikuDisplay;
        OnCollection += mainDisplay.Show;
        OnCompletion += ResetKanaCounter;

        // Init Kana Pickup System
        kanaPickupCollection = GameObject.FindGameObjectWithTag(kanaPickupCollectionTag).transform;
        GenerateKanaPickupsFor(haiku);
    }
    private void OnDestroy()
    {
        // Remove event connections
        OnCollection -= UpdateHaikuDisplay;
        OnCollection -= mainDisplay.Show;
        OnCompletion -= ResetKanaCounter;
    }

    #region Collection System
    private Haiku haiku;
    private Transform kanaPickupCollection;
    private const string kanaPickupCollectionTag = "Pickup Collection";
    private int KanaCollected = 0;

    public static event HaikuCompletionEventHandler OnCompletion;
    public static event HaikuCollectionEventHandler OnCollection;

    public void CollectKana(Kana collectedKana)
    {
        KanaCollected++;
        OnCollection?.Invoke(collectedKana);

        if (KanaCollected >= haiku.KanaCount)
        {
            CompleteHaiku();
        }
    }
    public void CompleteHaiku()
    {
        OnCompletion?.Invoke(haiku);
        SessionManager.Instance.BeginStageChange();
    }

    private void ResetKanaCounter() => KanaCollected = 0;
    private void ResetKanaCounter(Haiku haiku) => ResetKanaCounter();
        
    private void GenerateKanaPickupsFor(Haiku nextHaiku)
    {
        var allKana = nextHaiku.ToKana();
        if (allKana.Length > kanaPickupCollection.childCount)
        {
            Debug.LogError("Not enough pickups for haiku: " + nextHaiku.Name);
        }

        // Reset all pickups
        for (int k = 0; k < allKana.Length; k++)
        {
            var kanaPickup = kanaPickupCollection.GetChild(k).GetComponent<KanaPickup>();
            kanaPickup.SetActive(false);
        }

        // Init a pickup for each kana
        var pickupUsed = new bool[kanaPickupCollection.childCount];
        for (int k = 0; k < allKana.Length; k++)
        {
            // Get unique random index
            int uniquePickupIndex;
            do
            {
                uniquePickupIndex = Random.Range(0, kanaPickupCollection.childCount);                
            }
            while (pickupUsed[uniquePickupIndex] == true);

            // Init pickup at index
            var kanaPickup = kanaPickupCollection.GetChild(uniquePickupIndex).GetComponent<KanaPickup>();
            kanaPickup.SetKana(to: allKana[k]);
            pickupUsed[uniquePickupIndex] = true;
        }
    }
    #endregion

    #region Display
    private const float displayResetDelay = 0f;

    private void GeneratePickupsAfterDelay() => Invoke("GeneratePickups", displayResetDelay + 1f);
    private void GeneratePickups() => GenerateKanaPickupsFor(haiku);

    private void InitDisplayAfterDelay() => Invoke("InitDisplay", displayResetDelay + 0.5f);
    private void InitDisplay() =>  haikuDisplay.InitDisplay(haiku);

    private void ResetDisplayAfterDelay(Haiku haiku) => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplayAfterDelay() => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplay() => haikuDisplay.ResetDisplay();

    private void GetNewHaiku() => GetNewHaiku(haiku);
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