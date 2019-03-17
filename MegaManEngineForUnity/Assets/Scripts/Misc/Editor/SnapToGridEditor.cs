using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Misc_SnapToGrid))]
public class SnapToGridEditor : Editor
{

    private void OnSceneGUI()
    {
        Misc_SnapToGrid stg = (Misc_SnapToGrid)target;

        stg.SnapToPixelGrid();
    }

}
