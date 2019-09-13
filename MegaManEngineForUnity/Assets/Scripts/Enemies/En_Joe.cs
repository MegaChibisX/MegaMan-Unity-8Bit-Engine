using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_Joe : Enemy
{

    // We will put the mask over the current Joe sprite.
    // For that we will make a list of sprite pairs.
    // For each pair, if sprite X is shown from spriteSource,
    // sprite Y will be shown in spriteOutput.
    public SpritePair[] sprites;
    public SpriteRenderer spriteSource;
    public SpriteRenderer spriteOutput;

    public Animator anim;


    protected Collider2D col;


    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider2D>();

        if (GameManager.playerPosition.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);

        // We start the Joe behavior.
        StartCoroutine(Behavior());
    }
    protected void LateUpdate()
    {
        foreach (SpritePair s in sprites)
        {
            if (spriteSource.sprite == s.Key)
            {
                spriteOutput.sprite = s.Value;
                break;
            }
        }
    }
    public override void ChangeColorScheme(Color[] colors)
    {
        if (colors.Length > 0)
            spriteOutput.color = colors[0];
    }

    public IEnumerator Behavior()
    {
        while (true)
        {
            // Gives one frame delay. Always have at least one frame that
            // will certainly be delayed in while(true) loops within IEnumerators
            // to avoid the game freezing in case the IEnumerator goes in an infinite loop
            // by accident while debugging. Unity may freeze otherwise.
            yield return null;

            // The time Joe will wait before shooting.
            float time = 5.0f;

            while (time > 0.0f)
            {
                // By reducing the timer each frame and waiting for the next one,
                // we make the Sniper Joe wait.
                time -= Time.deltaTime;
                yield return null;

                // If MegaMan is behind the Joe, we want him to jump back.
                // This routine pauses and JumpBack plays.
                if ((GameManager.playerPosition.x - transform.position.x) * transform.localScale.x > 32)
                {
                    yield return JumpBack();
                    time = 2.5f;
                }
            }

            yield return Shoot();
        }
    }

    // Makes the Joe shoot.
    public IEnumerator Shoot()
    {
        anim.Play("Shoot");
        shielded = false;

        for (int i = 0; i < 3; i++)
        {
            // Makes the Joe wait for 0.5 seconds.
            // In this case there is no reason to define
            // a time variable, as we don't need the Joe to
            // do anything while waiting.
            yield return new WaitForSeconds(0.5f);

            Shoot((Vector2)transform.position - Vector2.right * transform.localScale.x * 10f, Vector2.left * transform.localScale.x, 2);
        }

        yield return new WaitForSeconds(0.5f);
        anim.Play("Idle");
        shielded = true;
    }
    // Makes the Joe jump back.
    public IEnumerator JumpBack()
    {
        // Stops any movement and makes the Joe jump.
        body.velocity = Vector2.zero;
        body.AddForce(transform.localScale.x * Vector2.right * 6000f + Vector2.up * 15000f);
        anim.Play("Jump");

        // Waits for Joe to touch the ground.
        while (!isGrounded)
            yield return null;

        // Resets its horizontal velocity.
        body.velocity = new Vector2(0, body.velocity.y);
        anim.Play("Idle");
    }



    public bool isGrounded
    {
        get
        {
            // If the object isn't moving upwards, then we check if there is a ground object underneath (Layer == 8).
            // If both are true, the Joe is on the ground.
            return //body.velocity.y <= 0f &&
                   Physics2D.OverlapBox((Vector2)col.bounds.center + Vector2.down * 4.0f,
                                         new Vector2(col.bounds.size.x * 0.8f, col.bounds.size.y), 0f, 1 << 8);
        }
    }

}
