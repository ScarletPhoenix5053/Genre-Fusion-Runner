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
    
    private void Awake()
    {
        ValidateSingleton();
    }
    private void Start()
    {
        GeneratePlaceHolderHaiku();

        InitDisplay();
        InitEvents();
        InitKanaPickupSystem();

        GenerateKanaPickupsFor(haiku);
    }
    private void GeneratePlaceHolderHaiku()
    {
        haiku = TestHaiku();
    }

    #region Collection System
    private Haiku haiku;

    public static event HaikuCollectionEventHandler OnCollection;
    public static event HaikuCompletionEventHandler OnCompletion;
    private void InitEvents()
    {

        //OnCollection += LogCollection;
        OnCollection += UpdateHaikuDisplay;
        OnCollection += mainDisplay.Show;

        //OnCompletion += LogCompletion;
        OnCompletion += ResetDisplayAfterDelay;
    }

    public Kana NextKana => haiku.ToKana()[KanaCollected];

    private int KanaCollected = 0;
    public void CollectKana(Kana collectedKana)
    {
        KanaCollected++;
        OnCollection?.Invoke(collectedKana);

        if (KanaCollected >= haiku.KanaCount())
        {
            OnCompletion.Invoke(haiku);
            KanaCollected = 0;
        }
    }

    [SerializeField] private KanaPickup kanaPickupObj;
    private Transform kanaPickupCollection;
    private const string kanaPickupCollectionTag = "Pickup Collection";
    private void InitKanaPickupSystem()
    {
        kanaPickupCollection = GameObject.FindGameObjectWithTag(kanaPickupCollectionTag).transform;
    }
    private void GenerateKanaPickupsFor(Haiku nextHaiku)
    {
        var allKana = nextHaiku.ToKana();
        var posZ = 0;
        for (int k = 0; k < allKana.Length; k++)
        {
            var kanaPickup = kanaPickupCollection.GetChild(k).GetComponent<KanaPickup>();
            kanaPickup.SetKana(to: allKana[k]);
        }
        /*
        foreach (Kana kana in allKana)
        {
            posZ += 5;
            var kanaPickup = 
                Instantiate(
                    kanaPickupObj,
                    Vector3.zero + Vector3.forward * posZ,
                    Quaternion.identity,
                    kanaPickupCollection
                    )
                    .GetComponent<KanaPickup>();
            kanaPickup.SetKana(to: kana);
        }*/
    }
    #endregion
    #region Display
    [Header("Display")]
    [SerializeField] MajorDisplay mainDisplay;
    private HaikuDisplay haikuDisplay;

    private void InitDisplay()
    {
        haikuDisplay = GetComponent<HaikuDisplay>();
        haikuDisplay.InitDisplay(haiku);
    }

    private const float displayResetDelay = 1f;
    private void ResetDisplayAfterDelay(Haiku haiku) => Invoke("ResetDisplay", displayResetDelay);
    private void ResetDisplay() => haikuDisplay.ResetDisplay();

    private void UpdateHaikuDisplay(Kana newKana)
    {
        Debug.Assert(newKana != null);
        haikuDisplay.SetKanaSquare(newKana.Line, newKana.PosInLine, to: newKana);
    }
    #endregion    

    #region Debug

    private void LogCollection(Kana kana)
    {
        Debug.Log("Collected a kana : " + kana + " " + KanaCollected + "/" + haiku.KanaCount());
    }
    private void LogCompletion(Haiku haiku)
    {
        Debug.Log("Yay you finished the haiku: " + haiku + "! " +  "by collecting " + KanaCollected + " kana!");
    }

    private const string testCsvFolder = "C:/Users/carlo/Documents/Programming/Kinetic Translation/Kinetic-Translation/Assets/Haiku Data/Resources/";
    private const string testCsvPath = "Haiku_SummerGrasses.csv";
    private static Haiku TestHaiku()
    {
        var newHaiku = new Haiku(testCsvFolder + testCsvPath);

        /*
        var newLines = new HaikuLine[]
        {
            new HaikuLine(
            new Kana[]
            {
                new Kana('蝶'),
                new Kana('の'),
                new Kana('触'),
                new Kana('れ')
            }, "The butterfly touches"),
            new HaikuLine(
            new Kana[]
            {
                new Kana('行'),
                new Kana('く'),
                new Kana('礎'),
                new Kana('沓'),
                new Kana('に')
            }, "and goes around the base stone"),
            new HaikuLine(
            new Kana[]
            {
                new Kana('匂'),
                new Kana('ふ'),
                new Kana('草')
            }, "and glass gives off the scent"),
        };
        var newHaiku = new Haiku(newLines);
        */

        /*
        Debug.Log(newHaiku);
        Debug.Log(newHaiku.KanaCount());
        Debug.Log(newHaiku.ToEnglish());
        Debug.Log(newHaiku.LineToEnglish(1));
        Debug.Log(newHaiku.LineToKana(2));
        Debug.Log(newHaiku.LineToString(3));
        */

        return newHaiku;
    }
    private static void TestLine()
    {
        var newLineKana = new Kana[]
        {
            new Kana('蝶'),
            new Kana('の'),
            new Kana('触'),
            new Kana('れ')
        };
        var meaning = "The butterfly touches";
        var newLine = new HaikuLine(newLineKana, meaning);
        Debug.Log(newLine);
        Debug.Log(newLine.Meaning);
    }
    private static void TestKana()
    {
        var newChar = '蜻';
        var newSound = "sei";
        var newMeaning = "dragonfly";
        var newKana = new Kana(newChar, newSound, newMeaning);
        Debug.Assert(newKana.ToString() == newChar.ToString());
        Debug.Assert(newKana.Character == newChar);
        Debug.Assert(newKana.Sound == newSound);
        Debug.Assert(newKana.Meaning == newMeaning);
        Debug.Log(newKana.ToString());
        Debug.Log(newKana.Sound);
        Debug.Log(newKana.Meaning);
    }
    #endregion
}
public delegate void HaikuCollectionEventHandler(Kana kana);
public delegate void HaikuCompletionEventHandler(Haiku haiku);