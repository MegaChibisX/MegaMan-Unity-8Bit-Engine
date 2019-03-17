using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base script all enemies inherit from. 
/// All enemies need a Rigidbody2D component.
/// </summary>
[AddComponentMenu("MegaMan/Enemy/Enemy")]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour {


    // Keeps track of the health, contact damage, if the enemy can be hit,
    // if the enemy is shielded, and if the enemy should be destroyed when it goes outside view.
    public float health = 1;
    public float damage = 1.0f;
    public bool canBeHit = true;
    public bool shielded = false;
    public bool destroyOutsideCamera = true;

    // The center position of the enemy. Usually set based on the collider,
    // if it is not set manually. Each enemy needs to set it on its own if it needs to.
    public Vector3 center;

    // The explosion the enemy should create once it dies. It is usually either small, big or big and hurting.
    public GameObject deathExplosion;

    // The rigidbody of the enemy, which handles physics.
    protected Rigidbody2D body;


    protected virtual void Start()
    {
        // Sets a reference for the rigidbody of this gameObject.
        body = GetComponent<Rigidbody2D>();
    }
    protected virtual void OnDrawGizmosSelected()
    {
        // Draws a sphere in the center of the enemy.
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + center, 2.0f);
    }


    public virtual void Damage(Pl_Weapon weapon)
    {
        // Unique reactions based on the weapon damage should be handled here.
        // Weaknesses and resistances, for example, should be done before calling Damage(float dmg, bool ignoreInvis).
        if (shielded)
        {
            weapon.Deflect();
            return;
        }
        Damage(weapon.damage, weapon.ignoreInvis);
    }
    public virtual void Damage(float dmg, bool ignoreInvis)
    {
        // Lowers the enemy's health and kills if needed.
        health -= dmg;
        if (health <= 0)
        {
            Kill(true);
        }
    }
    public virtual void Kill(bool makeItem)
    {
        // Creates an explosion at the center of the enemy.
        if (deathExplosion != null)
        {
            deathExplosion = Instantiate(deathExplosion);
            deathExplosion.transform.position = transform.position + center;
            deathExplosion.transform.rotation = transform.rotation;
            deathExplosion.transform.localScale = transform.localScale;
        }

        if (makeItem)
        {
            // Spawns a random item with the normal chances of spawning each item.
            GameObject item = Item.GetObjectFromItem(Item.GetRandomItem(1, 1, 1, 1, 1, 1, 1, 1));
            if (item)
            {
                item = Instantiate(item);
                item.transform.position = transform.position + center;
                item.transform.rotation = transform.rotation;
                item.transform.localScale = transform.localScale;
            }
        }

        Destroy(gameObject);
    }
    public virtual void Despawn()
    {
        // If the enemy was spawned by an EnemyBlueprint class, it will be despawnable, and despawning will be handled by it.
        if (destroyOutsideCamera)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Shoot(Vector2 shootPosition, Vector2 shotDirection, float shotDamage)
    {
        // Basic shooting code. Sets the position, direction and damage of the Default Shot.
        GameObject o = Instantiate((GameObject)Resources.Load("Prefabs/Enemies/En_Shot", typeof(GameObject)));
        o.transform.position = shootPosition;
        EnWp_Shot shot = o.GetComponent<EnWp_Shot>();
        shot.direction = shotDirection;
        shot.damage = shotDamage;
    }

    public virtual void LookAtPlayer()
    {
        // Looks at the player.
        if (GameManager.playerPosition.x > transform.position.x)
            transform.eulerAngles = Vector3.up * 180.0f;
        else
            transform.eulerAngles = Vector3.up * 0.0f;
    }
    public virtual Player GetClosestPlayer()
    {
        return Player.instance;
    }



}
