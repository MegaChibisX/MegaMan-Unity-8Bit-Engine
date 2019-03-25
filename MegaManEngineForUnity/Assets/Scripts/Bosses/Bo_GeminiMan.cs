using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_GeminiMan : Boss
{

    [Header("--------")]

    // Animator, collider, sprite renderer and audio for the boss.
    private Animator anim;
    private BoxCollider2D col;
    private SpriteRenderer spr;
    private AudioSource aud;

    // Other Gemini Man
    public Bo_GeminiMan otherGemini;
    public bool isPrimary = true;
    // Checks if the boss is currently doing something.
    public bool isBusy = false;
    // Checks if another Gemini needs everyone to not do a thing.
    public bool shouldFreeze = false;

    // Gemini Man's most well known weapon, the annoying laser.
    public GameObject GeminiLaser;


    // Gemini Man's jumps use two corners for reference.
    public Transform leftCorner;
    public Transform rightCorner;


    // Gemini Man's weapons
    public GameObject smallShot;

    // A simple hurt sprite that Gemini Man displays instead of his
    // animation when he is hurt.
    public Sprite hurtSprite;
    // A regular explosion that is used in Gemini Man's death for fanciness.
    public GameObject explosionObject;

    // These variables are only meant to be used by Jump(bool jumpBack, bool shoot, int hops, int maxHops).
    // However, placed here, they can be viewed as Gizmos for debug purposes.
    private Vector3 targetPositionJump;
    private Vector3[] targetPositionJumpAr;



    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_GeminiMan)
            Destroy(gameObject);

        // Warns the developer if some component is missing from the boss gameObject.
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("Gemini Man named " + name + "has no Animator!");
        col = GetComponentInChildren<BoxCollider2D>();
        if (col == null)
            Debug.LogWarning("Gemini Man named " + name + "has no BoxCollider2D!");
        spr = GetComponentInChildren<SpriteRenderer>();
        if (spr == null)
            Debug.LogWarning("Gemini Man named " + name + "has no Sprite Renderer!");
        aud = GetComponentInChildren<AudioSource>();
        if (aud == null)
            Debug.LogWarning("Gemini Man named " + name + "has no Audio Source!");

        // The boss starts frozen, so they need to be kinematic.
        body = GetComponent<Rigidbody2D>();
        body.isKinematic = true;

        // The boss needs both corners to be assigned to correctly calculate the jumps.
        if (leftCorner == null || rightCorner == null || leftCorner == rightCorner)
        {
            Debug.LogError("A corner transform has not been assigned, or both have been assigned to the same gameObject.");
        }

        if (isPrimary)
            otherGemini.GetComponentInChildren<SpriteRenderer>().enabled = false;
    }
    private void LateUpdate()
    {
        // If invisible, makes the boss flash.
        if (invisTime % 0.2f > 0.07f)
            spr.sprite = hurtSprite;
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }
    protected override void OnDrawGizmosSelected()
    {
        // Displays the desired jumps and makes it look nice.
        if (leftCorner && rightCorner)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(leftCorner.position * 1.00f + rightCorner.position * 0.00f, 4.0f);
            Gizmos.DrawSphere(leftCorner.position * 0.75f + rightCorner.position * 0.25f, 4.0f);
            Gizmos.DrawSphere(leftCorner.position * 0.50f + rightCorner.position * 0.50f, 4.0f);
            Gizmos.DrawSphere(leftCorner.position * 0.25f + rightCorner.position * 0.75f, 4.0f);
            Gizmos.DrawSphere(leftCorner.position * 0.00f + rightCorner.position * 1.00f, 4.0f);
        }
    }


    public override void Damage(float dmg, bool ignoreInvis)
    {
        // If can be hurt, get hurt.
        if (invisTime > 0.0f && !ignoreInvis || !fightStarted)
            return;

        health -= dmg;
        invisTime = 1f;
        if (health <= 0)
        {
            Kill(false);
        }
    }   
    public void Freeze(bool shouldBeTrue)
    {
        shouldFreeze = shouldBeTrue;
        body.bodyType = shouldFreeze ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
        body.constraints = shouldFreeze ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;
    }

    public override void Kill(bool makeItem)
    {
        // Gemini Man stops doing what he's doing and dies.
        StopAllCoroutines();
        StartCoroutine(PlayDeathLong());
    }


    public override IEnumerator PlayIntro()
    {

        // The fight can't start if it has already started.
        if (fightStarted)
            yield break;

        yield return null;

        // Sets up the boss.
        health = 0;
        canBeHit = false;
        fightStarted = true;
        body.isKinematic = true;
        // Sets the GameManager to register one more boss active.
        GameManager.bossesActive++;

        // Freezes the player.
        if (Player.instance != null)
            Player.instance.CanMove(false);


        // Plays intro.
        anim.Play("Intro");
        yield return new WaitForSeconds(1f);

        // Slowly gains health.
        for (int i = 0; i < 28; i++)
        {
            health++;
            yield return new WaitForSeconds(0.05f);
        }

        // Unfreezes player and self.
        body.isKinematic = false;
        otherGemini.body.isKinematic = false;

        if (Player.instance != null)
            Player.instance.CanMove(true);


        yield return new WaitForSeconds(0.4f);

        // Starts battle.
        canBeHit = true;
        StartCoroutine(Behavior());
    }

    public IEnumerator PlayDeathLong()
    {
        // Can't die if Gemini Man is not in a fight.
        if (!fightStarted)
            yield break;

        if (isPrimary && otherGemini)
        {
            GameObject exp = Instantiate(explosionObject);
            exp.transform.position = otherGemini.transform.position;

            Destroy(otherGemini.gameObject);
        }

        Freeze(true);
        // Registers death to GameManager.
        GameManager.bossesActive--;
        GameManager.bossDead_GeminiMan = true;
        // Plays death animation.
        invisTime = 0.0f;
        if (isGrounded)
            anim.Play("Death");
        else
            anim.Play("DeathAir");
        fightStarted = false;
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        yield return null;
        yield return null;
        body.velocity = Vector2.zero;

        // Freezes player.
        Time.timeScale = 0.0f;
        if (Player.instance != null)
        {
            Player.instance.CanMove(false);
            Player.instance.SetGear(false, false);
            Player.instance.RefreshWeaponList();
            Player.instance.canBeHurt = false;
        }

        yield return new WaitForSecondsRealtime(1.0f);

        // Explodes.
        for (int i = 0; i < 10; i++)
        {
            GameObject exp = Instantiate(explosionObject);
            exp.transform.position = transform.position + new Vector3(Random.Range(-64f, 64f), Random.Range(-64f, 64f), -10f);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // Explodes more.
        for (int i = 0; i < 5; i++)
        {
            GameObject exp = Instantiate(explosionObject);
            exp.transform.position = transform.position + new Vector3(Random.Range(-16f, 16f), Random.Range(-16f, 16f), -10f);
            if (i > 0)
                Destroy(exp.GetComponent<AudioSource>());

            // Each explosion spawns slightly after the next, even though they are
            // supposed to happen at the same time. Without pooling, this might
            // prevent some lag.
            yield return new WaitForSecondsRealtime(0.05f);
        }

        // Unfreezes the game.
        Time.timeScale = 1.0f;
        if (Player.instance != null)
        {
            Player.instance.CanMove(true);
            Player.instance.canBeHurt = true;
        }

        Destroy(gameObject);
    }


    protected IEnumerator Behavior()
    {
        otherGemini.StartCoroutine(otherGemini.Jump(false, false, 1, 1));
        otherGemini.GetComponentInChildren<SpriteRenderer>().enabled = true;
        yield return null;
        while (otherGemini.isBusy)
        {
            yield return null;
        }

        while (true)
        {
            // Checks if both self and clone are in the same corner. If yes, does a unique attack.
            if (!((GameManager.playerPosition.x < transform.position.x) ^ (GameManager.playerPosition.x < otherGemini.transform.position.x)))
            {
                // Makes the other Gemini jump to the other corner of the room.
                otherGemini.StartCoroutine(otherGemini.Jump(false, false, 2, 2));
                yield return null;
                // Waits a random amount of time.
                // RANDOMNESS IS PATTERNS IS DISCOURAGED IN MEGAMAN GAMES, DON'T GO CRAZY.
                float waitTime = Random.Range(0.1f, 0.75f);
                while (waitTime > 0)
                {
                    waitTime -= Time.deltaTime;
                    yield return null;
                }

                // Keeps track of the other Gemini's velocity, freezes them, shoots, then lets them continue their jump.
                Vector3 otherVel = otherGemini.body.velocity;
                yield return Shoot(true, 3, true, 0.25f, 0.1f, 0.15f);
                yield return null;
                otherGemini.body.velocity = otherVel;

                // Waits until the other Gemini is finished with their jump.
                while (otherGemini.isBusy)
                    yield return null;

                // Other Gemini shoots when it lands.
                otherGemini.StartCoroutine(otherGemini.Shoot(true, 3, true, 0.25f, 0.1f, 0.15f));
                yield return null;
                while (otherGemini.isBusy)
                    yield return null;
            }


            // Does a random jump.
                switch (Random.Range(0, 6))
            {
                case 0:
                    otherGemini.StartCoroutine(otherGemini.Jump(false, false, 1, 1));
                    yield return Jump(false, false, 1, 1);

                    while (otherGemini.isBusy)
                        yield return null;

                    break;
                case 1:
                    otherGemini.StartCoroutine(otherGemini.Jump(false, false, 2, 2));
                    yield return Jump(false, true, 2, 2);

                    while (otherGemini.isBusy)
                        yield return null;

                    break;
                case 2:
                    otherGemini.StartCoroutine(otherGemini.Jump(false, false, 1, 1));
                    yield return Jump(false, false, 1, 2);

                    yield return Jump(false, false, 1, 1);

                    while (otherGemini.isBusy)
                        yield return null;

                    break;
                case 3:
                case 4:
                    if ((leftCorner.position - transform.position).sqrMagnitude < (rightCorner.position - transform.position).sqrMagnitude)
                    {
                        otherGemini.StartCoroutine(otherGemini.Run(false));
                        yield return Jump(false, false, 1, 1);
                    }
                    else
                    {
                        otherGemini.StartCoroutine(otherGemini.Jump(false, false, 1, 1));
                        yield return Run(false);
                    }

                    yield return null;
                    while (otherGemini.isBusy)
                        yield return null;

                    break;
                case 5:
                    otherGemini.StartCoroutine(otherGemini.Shoot(true, 3, false, 0.25f, 0.1f, 0.15f));
                    yield return Jump(false, false, 1, 1);

                    while (otherGemini.isBusy)
                        yield return null;

                    otherGemini.StartCoroutine(otherGemini.Run(false));
                    yield return Shoot(true, 3, false, 0.25f, 0.1f, 0.15f);
                    //yield return Shoot(true, 4, false, 0.25f, 0.06f, 0.15f);
                    yield return ShootGemini(true, false, 0.25f, 0.15f);

                    break;
            }
        }
    }

    public IEnumerator Jump(bool jumpBack, bool shoot, int hops, int maxHops)
    {
        yield return null;

        isBusy = true;

        // Finds the farthest corner Gemini Man can jump to.
        Transform jumpTarget = rightCorner;
        if ((GameManager.playerPosition.x < transform.position.x) ^ jumpBack)
            jumpTarget = leftCorner;

        // Looks at the player.
        anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);

        // The distance of each hop.
        float distancePerHop = (jumpTarget.position.x - transform.position.x) / maxHops;

        // The position of each hop is found at the beginning of the game.
        targetPositionJumpAr = new Vector3[hops];
        for (int i = 0; i < hops; i++)
            targetPositionJumpAr[i] = new Vector3(transform.position.x + distancePerHop * (i + 1), jumpTarget.position.y);

        for (int i = 1; i <= hops; i++)
        {
            distancePerHop = (jumpTarget.position.x - transform.position.x) / (maxHops - i + 1);
            targetPositionJumpAr[i - 1] = new Vector3(transform.position.x + distancePerHop, jumpTarget.position.y);

            anim.Play("Jump");

            // The left and right corners don't account for heightened platforms in the arena,
            // so we find the actual position by raycasting from above the expected position.
            Vector2 targetPosition = new Vector3(transform.position.x + distancePerHop, jumpTarget.position.y);
            RaycastHit2D hit;

            if (hit = Physics2D.Raycast(targetPosition + Vector2.up * 128.0f, Vector2.down, 250.0f, 1 << 8))
                targetPosition.y = hit.point.y;
            targetPositionJump = targetPosition;


            // Doesn't jump until allowed to, and waits an extra frame.
            while (shouldFreeze || body.constraints == RigidbodyConstraints2D.FreezeAll)
                yield return null;
            yield return null;

            // We use the pretty handy LaunchVelocity to find the trajectory of Gemini Man for each jump.
            body.velocity = Helper.LaunchVelocity(transform.position + (Vector3)col.offset,
                                                  (Vector2)targetPositionJumpAr[i - 1] + Vector2.up * (col.size.y),
                                                  58.0f + 3f * hops + Mathf.Max(0f, (targetPosition.y - transform.position.y)) * 0.1f,
                                                  Physics2D.gravity.y * body.gravityScale);

            // Waits for Gemini Man to reach the peak of his jump, then shoots
            // if needed.
            yield return null;
            while (body.velocity.y >= 0)
                yield return null;

            // Shoots if needed.
            if (shoot && i == 0
                )
            {
                Vector3 otherVel = Vector3.zero;
                if (otherGemini)
                    otherVel = otherGemini.body.velocity;

                yield return ShootGemini(false, true, 0.1f, 0.1f);

                yield return null;
                otherGemini.body.velocity = otherVel;
            }

            // Waits for Gemini Man to hit the ground.
            while (!isGrounded)
                yield return null;

            // Stands to recover after a hop.
            anim.Play("Stand");
            body.velocity = Vector2.zero;
            yield return new WaitForSeconds(0.3f);

            while (shouldFreeze)
                yield return null;
        }

        isBusy = false;
    }
    public IEnumerator Run(bool toNear)
    {
        yield return null;

        isBusy = true;

        // Finds the farthest corner Gemini Man can jump to.
        Vector3 targetPosition = rightCorner.position;
        if ((GameManager.playerPosition.x < transform.position.x) ^ toNear)
            targetPosition = leftCorner.position;
        targetPosition.y = transform.position.y;

        anim.transform.localScale = new Vector3((targetPosition.x < transform.position.x) ? 1 : -1, 1, 1);

        anim.Play("Run");

        while (Mathf.Abs(targetPosition.x - transform.position.x) > 1f)
        {
            if (shouldFreeze)
            {
                body.velocity = Vector3.zero;
                yield return null;
                continue;
            }
            body.velocity = (targetPosition - transform.position).normalized * Mathf.Min(200f, Mathf.Abs(targetPosition.x - transform.position.x) / Time.fixedDeltaTime);
            yield return null;
        }

        yield return null;

        anim.Play("Stand");

        isBusy = false;
    }

    public IEnumerator Shoot(bool lookAtPlayer, int shotNumber, bool freezeOther, float waitBefore, float waitBetween, float waitAfter)
    {
        yield return null;
    
        if (lookAtPlayer)
            anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);

        if (isGrounded)
            anim.Play("ShootFront");
        else
            anim.Play("ShootJump");

        if (freezeOther)
            otherGemini.Freeze(true);
        body.isKinematic = true;
        isBusy = true;

        yield return new WaitForSeconds(waitBefore);

        for (int i = 0; i < shotNumber; i++)
        {
            Shoot(transform.position + center, transform.right * -anim.transform.localScale.x, 3, 400);

            yield return new WaitForSeconds(waitBetween);
        }
        yield return new WaitForSeconds(waitAfter);

        yield return null;
        body.isKinematic = false;
        if (freezeOther)
            otherGemini.Freeze(false);
        isBusy = false;
    }
    public IEnumerator ShootGemini(bool lookAtPlayer, bool freezeOther, float waitBefore, float waitAfter)
    {
        yield return null;

        if (lookAtPlayer)
            anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);

        if (isGrounded)
            anim.Play("ShootFront");
        else
            anim.Play("ShootJump");

        if (freezeOther)
            otherGemini.Freeze(true);
        body.isKinematic = true;
        isBusy = true;

        yield return new WaitForSeconds(waitBefore);


        for (int i = 0; i < 3; i++)
        {
            GameObject shot = Instantiate(GeminiLaser);
            shot.transform.position = transform.position - transform.right * anim.transform.localScale.x * (16f + i * 6);
            shot.GetComponent<SpriteRenderer>().flipX = transform.right.x * -anim.transform.localScale.x > 0;

            shot.GetComponent<EnWp_GeminiLaser>().shotOrder = i;
            if (i == 0)
                Helper.PlaySound(shot.GetComponent<EnWp_GeminiLaser>().geminiNoise);
        }

        yield return new WaitForSeconds(waitAfter);

        yield return null;
        body.isKinematic = false;
        if (freezeOther)
            otherGemini.Freeze(false);
        isBusy = false;
    }



    public bool isGrounded
    {
        // Checks if Gemini Man is touching the ground.
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.up, new Vector2(col.size.x * 0.9f, col.size.y), 0, 1 << 8);
        }
    }


}
