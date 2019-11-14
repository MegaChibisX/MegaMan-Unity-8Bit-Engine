using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small tornado that breaks up items. Powered version sticks to enemies and makes them drop small items periodically.
/// </summary>
public class PlWp_WindStorm : Pl_Shot
{

    private BoxCollider2D col;

    public bool stickToEnemy;
    private Enemy target;

    public override void Start()
    {
        base.Start();
        col = GetComponentInChildren<BoxCollider2D>();
    }
    public override void Update()
    {
        base.Update();

        // Change direction on wall contact.
        if (isWalled)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        // Move if nor stuck to an enemy.
        if (!target)
            body.velocity = new Vector2(speed * (transform.localScale.x > 0 ? 1 : -1), body.velocity.y);
        // Stick to an enemy.
        else
        {
            body.velocity = Vector2.zero;
            transform.position = target.transform.position + target.center;
        }
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        // When something is hit, if it is an enemy
        // and the enemy can be hurt, it hurts the enemy.
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Enemy>() && other.gameObject.layer != 15)
        {
            Enemy enemy = other.attachedRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                enemy.Damage(this);
                if (stickToEnemy && target == null)
                {
                    target = enemy;
                    StartCoroutine(ItemBarrage());
                }
                else if ((enemy.health > 0 && destroyOnBigEnemies) || destroyOnAllEnemies)
                {
                    Destroy(gameObject);
                }
            }
        }
        // If a solid surface is hit and the weapon
        // should be destroyed, it gets destroyed.
        if (other.gameObject.layer == 8)
        {
            if (destroyOnWalls)
                Destroy(gameObject);
        }


        // Break up a big item into smaller ones.
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Item_Pickup>() && !target)
        {
            Item_Pickup item = other.attachedRigidbody.GetComponent<Item_Pickup>();
            switch (item.itemToGive)
            {
                case Item.Items.bigHealth:
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.Items.smallHealth));
                        o.transform.position = item.transform.position + Vector3.right * (-1.5f + i) * 10;
                    }
                    break;
                case Item.Items.bigEnergy:
                    for (int i = 0; i < 2; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.Items.smallHealth));
                        o.transform.position = item.transform.position + Vector3.right * (-0.5f + i) * 10;
                    }
                    break;
                case Item.Items.bigGear:
                    for (int i = 0; i < 4; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 1, 0, 1, 0, 1, 0, 0)));
                        o.transform.position = item.transform.position + Vector3.right * (-1.5f + i) * 10;
                    }
                    break;
                case Item.Items.boltBig:
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 5, 1, 5, 1, 0, 0, 0)));
                        o.transform.position = item.transform.position + Vector3.right * (-2 + i) * 10;
                    }
                    break;
                default:
                    return;
            }
            Destroy(item.gameObject);
        }
    }

    private IEnumerator ItemBarrage()
    {
        // If attached to an enemy, drop random items, then go away.
        Destroy(GetComponent<Misc_DestroyAfterTime>());
        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector2.zero;
        for (int i = 0; i < 5; i++)
        {
            if (target == null)
            {
                break;
            }

            GameObject item = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 2, 0, 0, 0, 1, 0, 2)));
            item.transform.position = transform.position;

            yield return new WaitForSeconds(0.5f);
        }
        Destroy(gameObject);
    }



    private bool isWalled
    {
        get
        {
            return
                Physics2D.OverlapBox((Vector2)transform.position + col.offset + (Vector2)transform.right * transform.localScale.x * col.size.x * 0.5f,
                                            new Vector2(col.size.x, col.size.y * 0.9f), 0, 1 << 8);
        }
    }

}
