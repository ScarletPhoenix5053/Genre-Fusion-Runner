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

        // Text feild
        kanaDatabase.KanaDataText = EditorGUILayout.TextArea(kanaDatabase.KanaDataText, GUILayout.Height(500f));
        
        // Generate database button
        if (GUILayout.Button("Generate Database"))
        {
            kanaDatabase.GenerateDatabase();
        }

        // Render all kana
        kanaPrintout = kanaDatabase.GetPrintoutOfAllKana();
        if (kanaPrintout != null) EditorGUILayout.HelpBox(kanaPrintout, MessageType.None);
    }
}