using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWp_Bomb : Pl_Weapon
{

    public GameObject explosion;
    public float fuse = 5f;


    public override void Update()
    {
        base.Update();
        fuse -= Time.deltaTime;
        if (fuse <= 0.0f)
            Explode();
    }
    public override void Deflect()
    {
        Explode();
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (collision.otherRigidbody != null && collision.otherRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = collision.otherRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                if ((enemy.health > 0 && destroyOnBigEnemies) || destroyOnAllEnemies)
                    Explode();
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (collision.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Explode();
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
                    Explode();
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (other.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Explode();
        }
    }


    public void Explode()
    {
        explosion = Instantiate(explosion);
        explosion.transform.position = transform.position;
        if (explosion.GetComponent<Pl_Weapon>())
            explosion.GetComponent<Pl_Weapon>().damage = damage;

        Destroy(gameObject);
    }

}
