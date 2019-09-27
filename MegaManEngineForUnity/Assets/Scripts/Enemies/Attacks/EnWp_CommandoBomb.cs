using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_CommandoBomb : Enemy
{


    public float turnTime = 0.5f;

    public Vector2 startDir = Vector2.right;
    public Vector2 turnDir = Vector2.up;

    public float speed = 200f;


    protected override void Start()
    {
        base.Start();

        startDir.Normalize();
        turnDir.Normalize();
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * -startDir);
    }

    private void FixedUpdate()
    {
        if (turnTime > 0.0f)
        {
            turnTime -= Time.deltaTime;
            if (turnTime <= 0.0f)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90, Vector3.forward) * -turnDir);
            }
        }
        

        if (turnTime > 0)
            body.velocity = startDir * speed;
        else
            body.velocity = turnDir * speed;
    }

    public override void Kill(bool makeItem, bool makeBolt)
    {
        deathExplosion = Instantiate(deathExplosion);
        deathExplosion.transform.position = transform.position;
        deathExplosion.transform.rotation = Quaternion.AngleAxis(transform.localEulerAngles.z - 90f, Vector3.forward);
        deathExplosion.transform.localScale = transform.localScale;

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
            Kill(false, false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
            Kill(false, false);
    }


}
