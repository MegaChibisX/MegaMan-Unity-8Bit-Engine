using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all the in-game Player Weapons.
/// This includes the Lemons the Mega Buster shoots, the Pharaoh Shot
/// shots and the Gemini Laser pieces.
/// For the Player Weapon Data that controls the player's colors
/// and what and how the player shoots, go to Pl_WeaponData.cs.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Pl_Weapon : MonoBehaviour {


    public enum WeaponTypes { Normal, Pharaoh, Gemini, Metal, Star, Bomb, Wind, Galaxy, Commando, Rush }
    public WeaponTypes weaponType;


    // Each weapon needs to do damage.
    public float damage = 1;
    // Checks if the weapon can ignore invisibily from bosses/enemies.
    // Plz use it wisely.
    public bool ignoreInvis = false;
    public bool ignoreShield = false;

    // Checks if the weapon should be destroyed
    // when it hits a solid surface.
    public bool destroyOnWalls = false;
    // Checks if the weapon should be destroyed
    // when it hits any enemy.
    public bool destroyOnAllEnemies = false;
    // Checks if the weapon should be destroyed
    // when it hits an enemy with more health than
    // the damage of this weapon.
    public bool destroyOnBigEnemies = true;
    // Destroys the weapon if it's no longer inside the Camera view.
    public bool destroyOutsideView = false; 

    // Each Weapon needs to have a rigidbody,
    // and a Deflect Audio Clip is appreciated.
    [System.NonSerialized]
    public Rigidbody2D body;
    public AudioClip deflectClip;


    public virtual void Start()
    {
        // Sets the rigidbody.
        body = GetComponent<Rigidbody2D>();
    }
    public virtual void Update()
    {
        if (destroyOutsideView && !InView())
            Destroy(gameObject);
    }
    public virtual void Deflect()
    {
        // Changes the direction of the shot and
        // prevents it from interracting with anything else.
        damage = 0;
        destroyOnAllEnemies = false;
        destroyOnBigEnemies = false;
        destroyOnWalls = false;

        // Plays the sound and changes the velocity.
        Helper.PlaySound(deflectClip);
        body.velocity = new Vector2(-body.velocity.x, Mathf.Abs(body.velocity.x));
        gameObject.AddComponent<Misc_DestroyAfterTime>().time = 10.0f;
    }
    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (collision.otherRigidbody != null && collision.otherRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = collision.otherRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                enemy.Damage(this);
                if ((enemy.health > 0 && destroyOnBigEnemies) || destroyOnAllEnemies)
                    Destroy(gameObject);
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (collision.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Destroy(gameObject);
        }
    }
    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = other.attachedRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                enemy.Damage(this);
                if ((enemy.health > 0 && destroyOnBigEnemies) || destroyOnAllEnemies)
                    Destroy(gameObject);
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (other.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Destroy(gameObject);
        }
    }

    public bool InView()
    {
        CameraCtrl cmr = CameraCtrl.instance;

        if (!cmr)
            return false;

        int withinBounds = 0;

        // Checks if the Blueprint is in view.
            if (transform.position.x + 128f < cmr.transform.position.x + 104)
                withinBounds++;
            if (transform.position.x + 128f > cmr.transform.position.x - 104)
                withinBounds++;
            if (transform.position.y + 128f < cmr.transform.position.y + 104)
                withinBounds++;
            if (transform.position.y + 128f > cmr.transform.position.y - 104)
                withinBounds++;

        return withinBounds == 4;
    }

}
