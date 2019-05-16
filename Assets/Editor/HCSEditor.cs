using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HaikuCollectionSystem))]
public class HCSEditor : Editor
{
    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var hcs = target as HaikuCollectionSystem;

        if (GUILayout.Button("Collect next haiku"))
        {
            if (Application.isPlaying)
            hcs.CollectKana(hcs.NextKana);
        }
        
    }
    */
}