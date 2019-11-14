using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(GenerateBakedMeshChild))]
public class GenerateBakedMeshChildEditor : Editor
{

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate"))
        {
            GenerateBakedMeshChild gen = (GenerateBakedMeshChild)target;
            gen.Generate();
        }
    }

}
