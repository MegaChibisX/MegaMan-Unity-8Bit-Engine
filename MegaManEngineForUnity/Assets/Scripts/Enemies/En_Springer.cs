using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_Springer : Enemy
{

    public float speedNorm = 25f;
    public float speedHigh = 75f;

    public float maxLeft = -16f;
    public float maxRight = 16f;

    protected float boingTime = 0.0f;

    protected Animator anim;

    protected override void Start()
    {
        base.Start();

        if (GameManager.playerPosition.x < transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);

        // Makes the maxLeft and maxRight variables world coordinates from local ones.
        maxLeft += transform.position.x;
        maxRight += transform.position.x;

        // Gets the animator.
        anim = GetComponent<Animator>();
    }

    protected void Update()
    {
        if (boingTime > 0)
        {
            boingTime -= Time.deltaTime;

            if (boingTime <= 0.0f)
                anim.Play("Idle");
            return;
        }

        float desPos = transform.localScale.x < 0 ? maxLeft : maxRight;
        float speed = Mathf.Abs(GameManager.playerPosition.y - transform.position.y) < 16f ? speedHigh : speedNorm;

        transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, desPos, Time.deltaTime * speed), transform.position.y, transform.position.z);

        if (Mathf.Abs(transform.position.x - desPos) < 1f)
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        // If it hits the player, it goes boing.
        if (boingTime <= 0.0 && collision.attachedRigidbody != null && collision.attachedRigidbody.GetComponent<Player>() != null)
        {
            boingTime = 2.0f;
            anim.Play("Boing");
        }
    }


    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + Vector3.right * maxLeft, 4f);
        Gizmos.DrawSphere(transform.position + Vector3.right * maxRight, 4f);
        Gizmos.DrawLine(transform.position + Vector3.right * maxLeft,
                        transform.position + Vector3.right * maxRight);
    }

}
