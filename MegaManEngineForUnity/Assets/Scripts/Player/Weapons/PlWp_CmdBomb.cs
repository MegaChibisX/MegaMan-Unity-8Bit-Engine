using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bomb whose direction can be changed once. Explodes if it hits an enemy, or makes a shockwave if it hits a wall.
/// </summary>
public class PlWp_CmdBomb : Pl_Shot
{

    private bool moved = false;

    public GameObject generator;
    public GameObject explosion;



    public override void Update()
    {
        base.Update();

        // If the up or down key is pressed and the bullet  hasn't already changed direction, change direction.
        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.35f && !moved)
        {
            moved = true;
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                transform.eulerAngles = Vector3.forward * 90f;
            }
            else
            {
                transform.eulerAngles = Vector3.forward * -90f;
            }
            body.velocity = transform.right * speed;
        }
    }


    public override void OnTriggerEnter2D(Collider2D other)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = other.attachedRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                if ((enemy.health > 0 && destroyOnBigEnemies) || destroyOnAllEnemies)
                    Explode(true);
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (other.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Explode(false);
        }
    }



    private void Explode(bool forced)
    {
        GameObject expl = Object.Instantiate(forced ? explosion : generator);
        expl.transform.position = transform.position;
        expl.transform.eulerAngles = transform.eulerAngles + Vector3.forward * 90;

        if (forced)
        {
            expl.GetComponent<Pl_Weapon>().damage = damage;
            expl.GetComponent<Pl_Weapon>().weaponType = weaponType;
        }
        Destroy(gameObject);
    }

}
