using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicEnviroment))]
public class DynamicEnvironmentEditor : Editor
{
    /*
    public override void OnInspectorGUI()
    {   
        base.OnInspectorGUI();
        var dynEnvironment = target as DynamicEnviroment;

        // Environment State Drop-Down
        EnvironmentState envState;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Environment State");
        envState = (EnvironmentState)EditorGUILayout.EnumPopup(dynEnvironment.EnvironmentState);
        GUILayout.EndHorizontal();

        if (envState != dynEnvironment.EnvironmentState)
        {
            dynEnvironment.SetEnvironmentState(envState);
        }

    }
    */
}
