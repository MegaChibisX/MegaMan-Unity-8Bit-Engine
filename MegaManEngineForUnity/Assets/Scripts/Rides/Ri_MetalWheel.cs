using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ri_MetalWheel : Ride
{

    private bool touchedGround = false;

    private Rigidbody2D body;

    public override void Start()
    {
        base.Start();
        body = GetComponent<Rigidbody2D>();
        canBeRidden = false;
    }
    public override void Update()
    {
        if (rider != null && touchedGround)
        {

            if (rider.canMove)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    body.velocity = new Vector2(body.velocity.x, rider.jumpForce * Player.timeScale / GameManager.globalTimeScale * rider.up.y);
                }
                body.velocity = new Vector2(rider.input.x * 64f, body.velocity.y);
            }
            else
            {
                body.velocity = Vector2.zero;
                transform.position = rider.transform.position - seat.localPosition;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!touchedGround && collision.gameObject.layer == 8)
        {
            touchedGround = true;
            body.gravityScale = 100f;
            body.bodyType = RigidbodyType2D.Dynamic;
            transform.rotation = Quaternion.identity;
            canBeRidden = true;
        }
    }

}
