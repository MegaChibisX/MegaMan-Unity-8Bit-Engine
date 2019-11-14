using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This can be used for balls that are meant to bounce off walls.
/// </summary>
public class EnWp_BounceBall : Enemy
{

    // Just two sprites, one for the normal ball,
    // and one for the ball when it hits a wall.
    public Sprite ballIdle;
    public Sprite ballBouncing;

    

    // Balls can have a maximum amount of bounces,
    // a maximum time to explode, or both, until they die.
    public int maxBounces = 3;
    public float maxLifetime = 4.0f;
    // Bounce time keeps track of how long the bounce sprite
    // needs to be shown.
    private float bounceTime = 0.0f;


    protected override void Start()
    {
        base.Start();

        // A reference of the Sprite Renderer is needed to change the sprite.
        rend = GetComponentInChildren<SpriteRenderer>();
        if (rend == null)
        {
            Debug.LogWarning("There is no Sprite Renderer in BounceBall named " + name + "!");
        }
    }
    private void Update()
    {
        // Changes the sprite back to normal a little time after a bounce.
        if (bounceTime > 0.0f)
        {
            bounceTime -= Time.deltaTime;
            if (bounceTime <= 0.0f)
                rend.sprite = ballIdle;
        }

        // If alive for too long, kill.
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0)
        {
            Kill(false, false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the ball needs to bounce, then it bounces.
        if (Vector3.Angle(body.velocity, collision.GetContact(0).normal) > 90.0)
        {
            Vector2 reflectVel = Vector2.Reflect(body.velocity, collision.GetContact(0).normal);
            body.velocity = reflectVel;
            bounceTime = 0.6f;
            rend.sprite = ballBouncing;
        }
    }

}
