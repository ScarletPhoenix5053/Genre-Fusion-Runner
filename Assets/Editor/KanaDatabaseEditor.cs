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

        // Generate database button
        if (GUILayout.Button("Generate Database"))
        {
            kanaDatabase.GenerateDatabase();
            kanaPrintout = "";
            kanaPrintout = kanaDatabase.GetPrintoutOfAllKana();
        }

        // Render all kana
        EditorGUILayout.HelpBox(kanaPrintout, MessageType.None);
    }
}