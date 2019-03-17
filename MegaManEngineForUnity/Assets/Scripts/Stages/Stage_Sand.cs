using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The sand that makes jumping out of it awkward.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Sand")]
public class Stage_Sand : MonoBehaviour {

    // Limtis objects when their speed, when they move towards the direction 
    public float angle = 270.0f;
    public float sinkSpeed = 16;
    private Vector2 right;
    private Vector2 up;

    private void Start()
    {
        right = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
        up = Quaternion.AngleAxis(angle + 90.0f, Vector3.forward) * Vector2.right;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Limits any rigidbody's velocity.
        Rigidbody2D body = collision.attachedRigidbody;
        if (body)
        {
            Vector2 r = Vector2.Scale(body.velocity, right);
            Vector2 u = Vector2.Scale(body.velocity, up);
    
            if (body.velocity.y < 0)
                r = Vector2.Lerp(r, right * sinkSpeed, 0.7f);

            body.velocity = r + u;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta * 0.5f;
        Gizmos.DrawSphere(transform.position, 4.0f);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right * 64);
    }


}
