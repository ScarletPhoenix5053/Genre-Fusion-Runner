using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class Haiku
{
    public readonly string Name;
    public readonly Kana[][] Lines;
    public readonly string[] Roumaji;
    public readonly string[] Translation;

    public string Scene;

    public int KanaCount
    {
        get
        {
            var count = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                count += Lines[i].Length;
            }
            return count;
        }
    }

    public Haiku(string name, Kana[][] lines, string[] roumaji, string[] translation)
    {
        Name = name;
        Lines = lines;
        Roumaji = roumaji;
        Translation = translation;
    }

    public override string ToString()
    {
        // Render each line to string
        var kanaLines = new string[Lines.Length];
        for (int i = 0; i < Lines.Length; i++)
        {
            var kanaLine = "";
            for (int n = 0; n < Lines[i].Length; n++)
            {
                kanaLine += Lines[i][n].Character;
            }
            kanaLines[i] = kanaLine;
        }

        return Name + "\r\n"
            + kanaLines[0] + "/"
            + kanaLines[1] + "/"
            + kanaLines[2] + "\r\n"
            + Roumaji[0] + " / "
            + Roumaji[1] + " / "
            + Roumaji[2] + "\r\n"
            + Translation[0] + " / "
            + Translation[1] + " / "
            + Translation[2];
    }

    public Kana[] ToKana()
    {
        var allKana = new Kana[KanaCount];
        var currentKanaIndex = 0;
        for (int i = 0; i < Lines.Length; i++)
        {
            for (int n = 0; n < Lines[i].Length; n++)
            {
                allKana[currentKanaIndex] = Lines[i][n];
                currentKanaIndex++;
            }
        }
        return allKana;
    }
}

[CreateAssetMenu(fileName = "Haiku Database", menuName = "Haiku/Haiku Database")]
public class HaikuDatabase : ScriptableObject
{
    [Header("Dependencies")]
    [SerializeField]
    private KanaDatabase kanaDatabase;

    [Header("Path")]
    [SerializeField]
    [Tooltip("Directory of data file")]
    private string haikuDataPath;

    [Header("Scenes")]
    public string DefaultSceneName;
    public List<string> HaikuSceneNames;

    public string HaikuDataText { get; set; }
    public List<Haiku> Haiku { get; set; }

    private const int haikuNameIndex = 0;
    private const int haikuKanaIndex = 1;
    private const int haikuRoumajiIndex = 2;
    private const int haikuTranslationIndex = 3;

    public void GenerateHaiku()
    {
        // Validate haiku data path        
        if (!File.Exists(haikuDataPath))
        {
            Debug.LogWarning("File path: " + haikuDataPath + " does not point to a file.");
        }

        // Clear old list
        Haiku = new List<Haiku>();

        // Split file into entries
        var csvEntries = new List<string[]>();
        /*
        using (StreamReader reader = new StreamReader(haikuDataPath))
        {*/
        var lines = new List<string>();
        var currentLine = "";
        var currentLineN = 0;
        var haikuCsvData = HaikuDataText.Split('\n');

        for (int i = 0; i < haikuCsvData.Length; i++)
        {
            currentLine = haikuCsvData[i];
            if (currentLine != null)
            {
                currentLineN++;
                lines.Add(currentLine);
            }
        }

        Debug.Assert(lines.Count % 5 == 0);

        for (int i = 0; i < lines.Count; i+=5)
        {
            var csvEntry = new string[4];
            for (int line = 0; line < 4; line++)
            {
                csvEntry[line] = lines[i + line];
            }
            csvEntries.Add(csvEntry);
        }
        //}

        // For each entry:
        for (int entryIndex = 0; entryIndex < csvEntries.Count; entryIndex++)
        {
            var entry = csvEntries[entryIndex];
            // Lay out char arrays for haiku lines
            var kanaChars = new char[3][];
            var kanaCharsEntry = entry[haikuKanaIndex].Split(',');
            for (int i = 0; i < 3; i++)
            {
                kanaChars[i] = kanaCharsEntry[i].Replace("\r", string.Empty).ToCharArray();
            }

            // Convert to kana data structs
            kanaDatabase.GenerateDatabase();
            var haikuKana = new Kana[3][];
            for (int i = 0; i < 3; i++)
            {
                haikuKana[i] = new Kana[kanaChars[i].Length];
                for (int n = 0; n < kanaChars[i].Length; n++)
                {
                    // Look up kana database
                    var kana = kanaDatabase.RetrieveKana(kanaChars[i][n]);

                    // Create new class with blank fields if none found
                    if (kana == null)
                    {
                        kana = new Kana(kanaChars[i][n], "", "");
                    }

                    haikuKana[i][n] = kana;
                }
            }

            // Read additional data
            var haikuName = entry[haikuNameIndex];
            var haikuTranslation = entry[haikuTranslationIndex].Split(',');
            var haikuRoumaji = entry[haikuRoumajiIndex].Split(',');

            // Add new haiku to list
            var newHaiku = new Haiku(
                haikuName,
                haikuKana,
                haikuRoumaji,
                haikuTranslation
                );
            newHaiku.Scene = HaikuSceneNames[entryIndex];
            Haiku.Add(newHaiku);
        }
    }

    public Haiku GetStartHaiku()
    {
        if (Haiku == null) GenerateHaiku();

        if (Haiku.Count == 0) Debug.LogError("Should have at least one haiku parsed from csv");

        return Haiku[0];
    }
    public Haiku GetRandomHaiku()
    {
        var randomIndex = Random.Range(0, Haiku.Count);
        return Haiku[randomIndex];
    }

    public string[] GetHaikuPrintouts()
    {
        var returnStringArray = new string[Haiku.Count];
        for (int i = 0; i < Haiku.Count; i++)
        {
            returnStringArray[i] = Haiku[i].ToString();
        }
        return returnStringArray;
    }
}