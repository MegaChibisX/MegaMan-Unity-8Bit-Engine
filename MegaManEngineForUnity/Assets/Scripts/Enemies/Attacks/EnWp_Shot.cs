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

    public int frameSpeed = 12;
    public Sprite[] sprites;

    protected override void Start()
    {
        base.Start();
        if (direction.sqrMagnitude > 0)
            body.velocity = direction.normalized * speed;

        rend = GetComponentInChildren<SpriteRenderer>();
    }
    protected override void LateUpdate()
    {
        if (sprites != null && rend != null && sprites.Length > 0.0f)
            rend.sprite = sprites[(int)(Time.time * frameSpeed) % sprites.Length];
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnWall)
        {
            Kill(false, false);
        }
    }
    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnWall)
        {
            Kill(false, false);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 3.0f);
    }

}
