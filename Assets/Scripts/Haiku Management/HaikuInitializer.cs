using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaikuInitializer : MonoBehaviour
{
    private const string haikuDatabasePath = "Haiku Data/HaikuDatabase.data";
    private const string kanaDatabasePath = "Haiku Data/KanaDatabase.data";

    private void Awake()
    {
        var haikuDatabase = Resources.Load<HaikuDatabase>(haikuDatabasePath);
        var kanaDatabase = Resources.Load<KanaDatabase>(kanaDatabasePath);

        
        kanaDatabase.GenerateDatabase();
        haikuDatabase.GenerateHaiku();
    }
}
