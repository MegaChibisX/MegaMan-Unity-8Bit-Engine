using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyBlueprint))]
public class EnemyBlueprint_Editor : Editor
{


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EnemyBlueprint bp = (EnemyBlueprint)target;

        if (GUILayout.Button("Recalculate Bounds"))
        {
            bp.CalculateBounds();
        }
    }

}
