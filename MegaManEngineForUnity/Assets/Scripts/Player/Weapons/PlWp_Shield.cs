using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWp_Shield : Pl_Weapon
{

    public override void OnTriggerEnter2D(Collider2D other)
    {
        EnWp_Shot shot;
        if ((shot = other.GetComponent<EnWp_Shot>()) != null)
        {
            Destroy(other.attachedRigidbody.gameObject);
            Helper.PlaySound(deflectClip);
        }
        else
            base.OnTriggerEnter2D(other);
    }

}
