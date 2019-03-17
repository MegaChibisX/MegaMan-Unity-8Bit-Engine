using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the typical enemy shot.
/// The script is very basic.
/// </summary>
public class EnWp_Shot : Enemy
{

    // Direction, speed and wall phase through.
    public Vector2 direction = Vector2.right;
    public float speed = 200.0f;
    public bool destroyOnWall = false;


    protected override void Start()
    {
        base.Start();
        body.velocity = direction.normalized * speed;
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnWall)
        {
            Kill(false);
        }
    }
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnWall)
        {
            Kill(false);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 3.0f);
    }

}
