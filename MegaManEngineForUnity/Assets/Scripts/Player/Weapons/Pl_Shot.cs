using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The basic in-game Player Shot. It has a speed and a direction.
/// Most things the shot need to do are handled by the Pl_Weapon class already.
/// 
/// For the Player Weapon Data that controls the player's colors
/// and what and how the player shoots, go to Pl_WeaponData.cs.
/// </summary>
public class Pl_Shot : Pl_Weapon {


    public float speed = 300.0f;


    public override void Start()
    {
        base.Start();
        body.velocity = transform.right * speed * (transform.localScale.x > 0 ? 1 : -1);
    }


}
