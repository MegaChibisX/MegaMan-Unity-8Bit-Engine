using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWp_BlackHole : Pl_Weapon
{


    public float time = 3.0f;
    private bool damaging = false;
    private bool activated = false;

    private Collider2D col;


    public override void Start()
    {
        base.Start();
        col = GetComponentInChildren<Collider2D>();
    }
    public override void Update()
    {
        base.Update();
        time -= Player.deltaTime;
        if (time <=  0.0f && !activated)
        {
            StartCoroutine(Shockwave());
        }
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (collision.otherRigidbody != null && collision.otherRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = collision.otherRigidbody.GetComponent<Enemy>();
                if (damaging && enemy.health <= damage)
                    enemy.Kill(true, true);
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (collision.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Destroy(gameObject);
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
                if (damaging && enemy.health <= damage)
                    enemy.Kill(true, true);
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


    private IEnumerator Shockwave()
    {
        activated = true;
        col.enabled = false;
        yield return new WaitForFixedUpdate();
        damaging = true;
        col.enabled = true;
        yield return new WaitForFixedUpdate();
        Destroy(gameObject);
    }

}
