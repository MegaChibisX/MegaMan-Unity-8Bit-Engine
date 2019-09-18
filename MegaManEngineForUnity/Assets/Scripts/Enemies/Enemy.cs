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
        float damage = weapon.damage;
        if (shielded && !weapon.ignoreShield)
        {
            weapon.Deflect();
            return;
        }
        Damage(damage, weapon.ignoreInvis);
    }
    public virtual void Damage(float dmg, bool ignoreInvis)
    {
        // Lowers the enemy's health and kills if needed.
        health -= dmg;
        if (health <= 0)
        {
            Kill(true, true);
        }
    }
    public virtual void Kill(bool makeItem, bool makeBolt)
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
        if (makeBolt)
        {
            // Spawns a random bolt with the normal chances of spawning each item.
            GameObject item = Item.GetObjectFromItem(Item.GetRandomBolt(1, 1, 1, 1));
            if (item)
            {
                item = Instantiate(item);
                item.transform.position = transform.position + center;
                item.transform.rotation = transform.rotation;
                item.transform.localScale = transform.localScale;

                item.GetComponent<Rigidbody2D>().AddForce(transform.up * Random.Range(-15000, 15000f) + transform.right * Random.Range(0, 8000f));
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

    public virtual GameObject Shoot(Vector2 shootPosition, Vector2 shotDirection, float shotDamage, float shotSpeed = 200, GameObject obj = null)
    {
        // This script is supposed to work with a GameObject containing a EnWp_Shot component or one that derives from it,
        // but can be used with any GameObject.

        // If there isn't a GameObject given, the the default bullet will be assigned.
        if (obj == null)
            obj = (GameObject)Resources.Load("Prefabs/Enemies/En_Shot", typeof(GameObject));

        // Makes a new instance of the bullet in the scene, then moves it and rotates it accordingly.
        GameObject o = Instantiate(obj);
        o.transform.position = shootPosition;
        if (shotDirection.sqrMagnitude > 0)
            o.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(shotDirection.y, shotDirection.x), Vector3.forward);

        // Checks if there is a EnWp_Shot component, and changes its properties accordingly.
        EnWp_Shot shot = o.GetComponent<EnWp_Shot>();
        if (shot != null)
        {
            if (shotDirection.sqrMagnitude > 0)
                shot.direction = shotDirection.normalized;
            shot.damage = shotDamage;
            shot.speed = shotSpeed;
        }
        // Since bullets shouldn't have gravity by default, the gravity is set to 0.
        if (o.GetComponent<Rigidbody2D>() != null)
            o.GetComponent<Rigidbody2D>().gravityScale = 0.0f;

        return o;
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
    public virtual void  ChangeColorScheme(Color[] colors) { }


}

[System.Serializable]
public struct SpritePair
{
    public Sprite Key;
    public Sprite Value;
}
