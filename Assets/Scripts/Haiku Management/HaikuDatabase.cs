using UnityEngine;
using System.IO;
using System.Collections.Generic;

public struct Haiku
{
    public readonly string Name;
    public readonly Kana[][] Lines;
    public readonly string[] Roumaji;
    public readonly string[] Translation;
    //public readonly Effect[] Effects;

    public Haiku(string name, Kana[][] lines, string[] roumaji, string[] translation)
    {
        Name = name;
        Lines = lines;
        Roumaji = roumaji;
        Translation = translation;
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

    private List<Haiku> Haiku;

    public void GenerateHaiku()
    {
        // Validate haiku data path

        // Clear old list

        // Split file into entries

        // For each entry:
        
            // Lay out char arrays for haiku lines
    
            // Convert to kana data structs

                // Look up kana database
                // Create new struct with blank fields if none found


            // Read translation/roumaji data

            // Add new haiku to list
        

    }
}