using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoMan : Player
{

    public Collider2D shieldCol;


    protected override void HandlePhysics_Movement()
    {
        base.HandlePhysics_Movement();

        if (shieldCol != null)
        {
            if (state == PlayerStates.Normal && !isGrounded && canAnimate)
                shieldCol.enabled = true;
            else
                shieldCol.enabled = false;
        }
    }

}
