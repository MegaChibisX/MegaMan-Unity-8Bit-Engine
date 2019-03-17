using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Obsolete script. If still present in the engine, blame a lazy or forgetful MegaChibisX.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Move Other With Self")]
[RequireComponent(typeof(Rigidbody2D))]
public class Misc_MoveOthersWithSelf : MonoBehaviour
{


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody && !collision.rigidbody.transform.parent)
        {
            //collision.rigidbody.transform.parent = transform;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.rigidbody.transform.parent == transform)
            collision.rigidbody.transform.parent = null;
    }
}
