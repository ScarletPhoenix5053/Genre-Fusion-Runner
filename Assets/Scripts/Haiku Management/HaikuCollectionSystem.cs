using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        ValidateSingleton();
    }
    private void Start()
    {
        haiku = haikuDatabase.GetStartHaiku();

        // Init Display
        haikuDisplay = GetComponent<HaikuDisplay>();
        haikuDisplay.InitDisplay(haiku);

        // Init Events
        OnCollection += UpdateHaikuDisplay;
        OnCollection += mainDisplay.Show;        
        OnCompletion += ResetDisplayAfterDelay;

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
        var posZ = 0;
        for (int k = 0; k < allKana.Length; k++)
        {
            var kanaPickup = kanaPickupCollection.GetChild(k).GetComponent<KanaPickup>();
            kanaPickup.SetKana(to: allKana[k]);
        }
    }
    #endregion

    #region Display

    private const float displayResetDelay = 1f;
    private void ResetDisplayAfterDelay(Haiku haiku) => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplay() => haikuDisplay.ResetDisplay();

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