using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KanaDatabase))]
public class KanaDatabaseEditor : Editor
{
    private string kanaPrintout;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var kanaDatabase = target as KanaDatabase;
        EditorUtility.SetDirty(kanaDatabase);

        // Text feild
        kanaDatabase.KanaDataTextE = EditorGUILayout.TextArea(kanaDatabase.KanaDataTextE, GUILayout.Height(500f));
        
        // Generate database button
        if (GUILayout.Button("Generate Database"))
        {
            kanaDatabase.GenerateDatabase();
            PlayerPrefs.SetString("kanaData", kanaDatabase.KanaDataTextE);
        }

        // Render all kana
        kanaPrintout = kanaDatabase.GetPrintoutOfAllKana();
        if (kanaPrintout != null) EditorGUILayout.HelpBox(kanaPrintout, MessageType.None);
    }
}