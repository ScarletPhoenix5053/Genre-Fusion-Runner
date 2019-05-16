using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public struct Kana
{
    public readonly char Character;
    public readonly string Sound;
    public readonly string Translation;

    public Kana(char character, string sound, string translation)
    {
        Character = character;
        Sound = sound;
        Translation = translation;
    }

    public override string ToString() => Character + " / " + Sound + " / " + Translation;
}

[CreateAssetMenu(fileName = "Kana Database", menuName = "Haiku/Kana Database")]
public class KanaDatabase : ScriptableObject
{
    [Header("Path")]
    [SerializeField]
    [Tooltip("Directory of data file")]
    private string kanaDataPath;

    private Dictionary<char, Kana> kanaDictionary;

    public void GenerateDatabase()
    {
        // Make sure file exists
        if (!File.Exists(kanaDataPath))
        {
            Debug.LogWarning("File path: " + kanaDataPath + " does not point to a file.");
        }

        // Make sure file is accessible
        //File.SetAttributes(Reques, FileAttributes.Normal);

        // Loop thru each line and write kana data
        using (StreamReader reader = new StreamReader(kanaDataPath))
        {
            var currentLine = "";
            var kanaData = new List<Kana>();

            do
            {
                currentLine = reader.ReadLine();
                if (currentLine != null)
                {
                    var kanaLine = currentLine.Split(',');
                    Debug.Assert(kanaLine.Length == 3);

                    var newKana = new Kana(
                        kanaLine[0].ToCharArray()[0],
                        kanaLine[1],
                        kanaLine[2]
                        );

                    kanaData.Add(newKana);
                }
            }
            while (currentLine != null);

            // Store in database
            kanaDictionary = new Dictionary<char, Kana>();
            foreach (Kana kana in kanaData)
            {
                kanaDictionary.Add(kana.Character, kana);
            }

            // Log all to console
        }
    }

    public Kana RetrieveKana(char kanaCharacter)
    {
        throw new System.NotImplementedException();
    }

    public string GetPrintoutOfAllKana()
    {
        var printout = "";
        
        foreach (KeyValuePair<char, Kana> kanaEntry in kanaDictionary)
        {
            printout += ("Key: " + kanaEntry.Key + " | Value: " + kanaEntry.Value) + "\r\n";
        }
        printout = printout.TrimEnd(new char[] { '\r', '\n' });

        return printout;
    }
}