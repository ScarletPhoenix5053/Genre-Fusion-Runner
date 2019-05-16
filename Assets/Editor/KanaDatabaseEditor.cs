using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KanaDatabase))]
public class KanaDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var kanaDatabase = target as KanaDatabase;

        // Generate database button
        if (GUILayout.Button("Generate Database"))
        {
            kanaDatabase.GenerateDatabase();
        }

        // Render out full database in editor
    }
}