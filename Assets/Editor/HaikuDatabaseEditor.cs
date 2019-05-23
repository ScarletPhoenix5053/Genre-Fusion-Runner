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

        // Text feild
        haikuDatabase.HaikuDataText = EditorGUILayout.TextArea(haikuDatabase.HaikuDataText, GUILayout.Height(500f));

        // Generate database button
        if (GUILayout.Button("Generate Haiku"))
        {
            haikuDatabase.GenerateHaiku();
        }

        // Render all haiku + motion profile edit options
        if (haikuDatabase.Haiku != null)
        {
            var haikuList = haikuDatabase.Haiku;
            for (int i = 0; i < haikuList.Count; i++)
            {
                EditorGUILayout.HelpBox(haikuList[i].ToString(), MessageType.None);
            }
        }

        // warn if default effect is empty
        if (haikuDatabase.DefaultSceneName == null)
        {
            EditorGUILayout.HelpBox(
                "Please assign a default effect.",
                MessageType.Error
                );
        }

        // warn if haiku list length is longer than fx list
        if (haikuDatabase.Haiku != null && haikuDatabase.HaikuSceneNames != null)
        {
            if (haikuDatabase.Haiku.Count > haikuDatabase.HaikuSceneNames.Count)
            {
                EditorGUILayout.HelpBox(
                    "There are more haiku than effects. Haiku without a" +
                    "corrosponding effect will be given the default effect.",
                    MessageType.Warning
                    );
            }
        }
    }
}