using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CopySkeleton))]
public class CopySkeleton_Editor : Editor
{


    public override void OnInspectorGUI()
    {
        CopySkeleton cs = (CopySkeleton)target;

        base.OnInspectorGUI();
        if (GUILayout.Button("Make child"))
        {
            cs.SetTargetBonesToSource(true);
        }
    }


}
