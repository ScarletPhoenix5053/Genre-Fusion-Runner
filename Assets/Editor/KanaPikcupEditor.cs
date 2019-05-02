using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KanaPickup))]
public class KanaPikcupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var kpe = target as KanaPickup;

        if (GUILayout.Button("Pickup Kana"))
        {
            if (Application.isPlaying)
            {
                kpe.Activate();
            }
        }
    }
}