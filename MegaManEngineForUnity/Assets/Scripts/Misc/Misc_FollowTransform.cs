using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script that chases another Transform, ignoring collisions.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Follow Transform")]
public class Misc_FollowTransform : MonoBehaviour {


    public Transform target;
    public Vector3 displacement;

    [Range(0f, 1f)]
    public float moveSpeed = 0.5f;


    private void Update()
    {
        if (!target)
            return;


        transform.position = Vector3.Lerp(transform.position, target.position + displacement, moveSpeed);
    }


    private void OnDrawGizmosSelected()
    {
        if (target)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(target.position + displacement, 4.0f);
        }
    }


}
