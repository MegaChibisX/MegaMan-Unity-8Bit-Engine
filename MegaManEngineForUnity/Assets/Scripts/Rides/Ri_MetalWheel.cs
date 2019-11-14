using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Metal Blade's powered version, the Metal Wheel. It does good damage
/// and the player can ride it to victory. It can't be ridden through sand though.
/// </summary>
public class Ri_MetalWheel : Ride
{

    private bool touchedGround = false;

    private Rigidbody2D body;
    private CircleCollider2D col;

    public override void Start()
    {
        base.Start();
        body = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        canBeRidden = false;
    }
    public override void Update()
    {
        // If there is a rider, move and jump.
        if (rider != null && touchedGround)
        {

            if (rider.canMove)
            {
                if (Input.GetButtonDown("Jump") && isGrounded)
                {
                    body.velocity = new Vector2(body.velocity.x, rider.jumpForce * Time.timeScale / GameManager.globalTimeScale * rider.up.y);
                }
                body.velocity = new Vector2(rider.input.x * 64f, body.velocity.y);
            }
            else
            {
                body.velocity = Vector2.zero;
                transform.position = rider.transform.position - seat.localPosition;
            }
        }
        if (isInSand)
            body.velocity = new Vector2(0, body.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!touchedGround && collision.gameObject.layer == 8)
        {
            touchedGround = true;
            body.gravityScale = 100f * transform.localScale.y;
            body.bodyType = RigidbodyType2D.Dynamic;
            transform.rotation = Quaternion.identity;
            canBeRidden = true;
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Stage_Sand>() != null)
        {
            float sinkSpeed = collision.GetComponent<Stage_Sand>().sinkSpeed * 0.25f;

            if (body.velocity.y * transform.localScale.y <= 0)
                body.velocity = new Vector2(0, Mathf.Clamp(body.velocity.y, -sinkSpeed, sinkSpeed));
        }
    }

    public bool isGrounded
    {
        get
        {
            return Physics2D.OverlapPoint(transform.position - transform.up * transform.localScale.y * col.radius * 1.1f, 1 << 8);
        }
    }
    public bool isInSand
    {
        get
        {
            return Physics2D.OverlapCircle(transform.position, col.radius, 1 << 12);
        }
    }

}
