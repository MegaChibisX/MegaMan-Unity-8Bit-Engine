using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic bomb. Will bounce. Can make explosions in every bounce.
/// To make the bomb bounce, you  will need to use a Bouncy Physics Material.
/// </summary>
public class PlWp_Bomb : Pl_Weapon
{

    // Explosion.
    public GameObject explosion;
    // Behavior  data.
    public float fuse = 5f;
    public bool explodeOnEveryBounce = false;
    private float cooldown = 0.25f;


    public override void Update()
    {
        // Explodes if the timer reaches zero.
        base.Update();
        fuse -= Time.deltaTime;
        if (fuse <= 0.0f)
            Explode();
        
        // There is a small cooldown for the bouncy explosions, to not make too loud and powerful.
        if (cooldown > 0.0f)
            cooldown -= Time.unscaledDeltaTime;
    }
    public override void Deflect()
    {
        // Can't deflect this one.
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
            else if (explodeOnEveryBounce)
                ExplodeHarmlessly();
        }
    }


    public void Explode()
    {
        explosion = Instantiate(explosion);
        explosion.transform.position = transform.position;
        if (explosion.GetComponent<Pl_Weapon>())
        {
            explosion.GetComponent<Pl_Weapon>().damage = damage;
            explosion.GetComponent<Pl_Weapon>().weaponType = weaponType;
        }

        Destroy(gameObject);
    }
    public void ExplodeHarmlessly()
    {
        if (cooldown > 0.0f)
            return;

        GameObject exp = Instantiate(explosion);
        exp.transform.position = transform.position;
        if (exp.GetComponent<Pl_Weapon>())
            exp.GetComponent<Pl_Weapon>().damage = 1;

        cooldown = 0.5f;
    }

}
