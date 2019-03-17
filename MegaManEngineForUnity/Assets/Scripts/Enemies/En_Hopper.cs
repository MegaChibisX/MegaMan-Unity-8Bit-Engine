using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script used by enemies that need to hop around.
/// The Met Mommy's children use it, for example.
/// </summary>
[AddComponentMenu("MegaMan/Enemy/Hopper")]
public class En_Hopper : Enemy
{

    // The height of the enemy.
    public float height = 16.0f;

    // The direction the enemy should hop at,
    // delay between hops and force of each jump
    public Vector2 hopDirection = Vector2.up;
    public float timeBetweenHops = 1.0f;
    public float jumpForce = 1000.0f;

    private float time = 0.0f;


    private void Update()
    {
        // If touching the ground, the hopper counts down until it needs to jump again.
        if (isGrounded)
        {
            if (body.velocity.y >= 0.0f && time <= 0.0f)
            {
                body.velocity = hopDirection.normalized * jumpForce;
                time = timeBetweenHops;
            }
            else
            {
                time -= Time.deltaTime;
            }
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.DrawLine(transform.position, transform.position - transform.up * height);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)hopDirection * 3);
    }



    protected bool isGrounded
    {
        get
        {
            return Physics2D.Linecast(transform.position, transform.position - transform.up * height, 1 << 8);
        }
    }

}
