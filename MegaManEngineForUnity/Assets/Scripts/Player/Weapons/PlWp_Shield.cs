using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic shield script. Deflects enemy shots. Not the rotating variety like Star Man's,  but the held one like Proto Man's.
/// </summary>
public class PlWp_Shield : Pl_Weapon
{

    public override void OnTriggerEnter2D(Collider2D other)
    {
        // If it's an enemy shot or a child of  one, destroy it.
        if (other.GetComponent<EnWp_Shot>() != null)
        {
            Destroy(other.attachedRigidbody.gameObject);
            Helper.PlaySound(deflectClip);
        }
        else
            base.OnTriggerEnter2D(other);
    }

}
