using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Camera borders that the CameraCtrl can use.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Change Camera Borders")]
public class Stage_ChangeCameraBorders : MonoBehaviour {


    public Vector2 leftCenter = Vector2.zero;
    public float maxRightMovement = 0.0f;
    public float maxUpMovement = 0.0f;






    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(leftCenter + Vector2.right * (maxRightMovement * 0.5f + 8.0f) + Vector2.up * (maxUpMovement * 0.5f + 8.0f), new Vector3(maxRightMovement + 272.0f, 240.0f + maxUpMovement, 0.0f));
    }

}

