using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_BowlieBall : Enemy
{


    public Sprite ball1;
    public Sprite ball2;

    private bool isActive;
    private float time = 0.0f;


    protected override void Start()
    {
        base.Start();
        // Finds the Sprite Renderer of the ball.
        rend = GetComponentInChildren<SpriteRenderer>();
    }

    public void Activate()
    {
        // Releases the transform from the bowlie.
        transform.parent = transform.parent.parent;

        // Lets the object fall and interact with physics.
        body.bodyType = RigidbodyType2D.Dynamic;

        // Activates the ball.
        isActive = true;

        // Tells the object to be destroyed after some time, just in case it falls through a hole.
        gameObject.AddComponent<Misc_DestroyAfterTime>().time = 20f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If a solid object is hit (layer == 8) and the ball is active, the ball gets destroyed.
        if (collision.gameObject.layer == 8 && isActive)
        {
            Kill(false, false);
        }
    }
    protected override void LateUpdate()
    {
        // Doesn't change sprite if the ball is not active.
        // It's usually not recommended to put 'return' in methods like LateUpdate() or Start(), but this is a fairly simple script.
        if (!isActive)
            return;

        // Plays the propeller animation based on a timer. No need for an Animator for such simple animations.
        time += Time.deltaTime;
        if (time > 0.2f)
            time %= 0.2f;

        if (time < 0.1f)
            rend.sprite = ball1;
        else
            rend.sprite = ball2;
    }

}
