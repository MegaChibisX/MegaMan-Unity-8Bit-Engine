using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_ShotAccel : Enemy {

    // Direction, speed and wall phase through.
    public Vector2 direction = Vector2.right;
    public float speed = 200.0f;
    private float curSpeed = 0f;
    public bool destroyOnWall = false;

    public int frameSpeed = 12;
    public Sprite[] sprites;

    protected override void Start()
    {
        base.Start();

        rend = GetComponentInChildren<SpriteRenderer>();
    }
    protected void FixedUpdate()
    {
        if (curSpeed < speed)
        {
            curSpeed += 200f * Time.fixedDeltaTime;
            curSpeed = Mathf.Clamp(curSpeed, 0, speed);

        }

        Quaternion targetRot = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, GameManager.playerPosition - transform.position));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, curSpeed * 0.75f * Time.fixedDeltaTime);
        direction = transform.right;

        if (direction.sqrMagnitude > 0)
            body.velocity = direction.normalized * curSpeed;
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
