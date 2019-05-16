using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HaikuDatabase))]
public class HaikuDatabaseEditor : Editor
{
    private string[] haikuPrintouts;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var haikuDatabase = target as HaikuDatabase;

        // Generate database button
        if (GUILayout.Button("Generate Haiku"))
        {
            haikuDatabase.GenerateHaiku();
        }

        // Render all haiku + motion profile edit options
        {
            if (haikuDatabase.Haiku == null) return;

            var haikuList = haikuDatabase.Haiku;
            for (int i = 0; i < haikuList.Count; i++)
            {
                EditorGUILayout.HelpBox(haikuList[i].ToString(), MessageType.None);
            }
        }
    }
}