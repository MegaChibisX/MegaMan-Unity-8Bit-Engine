using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (isWalled)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        if (!target)
            body.velocity = new Vector2(speed * (transform.localScale.x > 0 ? 1 : -1), body.velocity.y);
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
        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Enemy>())
        {
            Enemy enemy = other.attachedRigidbody.GetComponent<Enemy>();
            if (enemy.canBeHit)
            {
                enemy.Damage(this);
                if (stickToEnemy && target == null)
                {
                    target = enemy;
                    print(target);
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

        if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<Item_Pickup>() && !target)
        {
            Item_Pickup item = other.attachedRigidbody.GetComponent<Item_Pickup>();
            switch (item.itemToGive)
            {
                case Item.Items.bigHealth:
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.Items.smallHealth));
                        o.transform.position = item.transform.position + Vector3.right * (-2 + i) * 10;
                    }
                    break;
                case Item.Items.bigEnergy:
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.Items.smallEnergy));
                        o.transform.position = item.transform.position + Vector3.right * (-2 + i) * 10;
                    }
                    break;
                case Item.Items.bigGear:
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 1, 0, 1, 0, 1, 0, 0)));
                        o.transform.position = item.transform.position + Vector3.right * (-2 + i) * 10;
                    }
                    break;
                case Item.Items.boltBig:
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject o = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 2, 1, 2, 1, 0, 0, 0)));
                        o.transform.position = item.transform.position + Vector3.right * (-2 + i) * 10;
                    }
                    break;
                default:
                    return;
            }
            Destroy(item.gameObject);
        }
    }

    private  IEnumerator ItemBarrage()
    {
        Destroy(GetComponent<Misc_DestroyAfterTime>());
        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector2.zero;
        for (int i = 0; i < 5; i++)
        {
            if (target == null)
            {
                break;
            }

            GameObject item = Instantiate(Item.GetObjectFromItem(Item.GetRandomItem(0, 2, 1, 2, 1, 1, 0.5f, 2)));
            item.transform.position = transform.position;

            yield return new WaitForSeconds(0.5f);
        }
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
