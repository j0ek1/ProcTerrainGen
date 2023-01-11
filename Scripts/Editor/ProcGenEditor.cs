using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProcGenController))]
public class ProcGenEditor : Editor // Script to be used in editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Regenerate Textures")) // Create button and check if pressed
        {
            ProcGenController targetController = serializedObject.targetObject as ProcGenController;
            targetController.RegenerateTextures();
        }

        if (GUILayout.Button("Regenerate Terrain")) // Create button and check if pressed
        {
            ProcGenController targetController = serializedObject.targetObject as ProcGenController;
            targetController.RegenerateTerrain();
        }
    }
}
