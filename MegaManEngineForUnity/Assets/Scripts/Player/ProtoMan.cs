using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MegaMan/Allies/ProtoMan")]
public class ProtoMan : Player
{

    public Collider2D shieldCol;


    protected override void HandlePhysics_Movement()
    {
        base.HandlePhysics_Movement();

        // If Proto Man is mid-air and not attacking, activate the shield.
        if (shieldCol != null)
        {
            if (state == PlayerStates.Normal && !isGrounded && canAnimate)
                shieldCol.enabled = true;
            else
                shieldCol.enabled = false;
        }
    }

}
