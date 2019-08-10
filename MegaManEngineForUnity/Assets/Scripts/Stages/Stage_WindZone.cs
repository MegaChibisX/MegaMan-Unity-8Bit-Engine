using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("MegaMan/Stage/Wind Zone")]
public class Stage_WindZone : MonoBehaviour {

    public float strength = 16;
    public Vector2 direction = Vector2.right;


}
