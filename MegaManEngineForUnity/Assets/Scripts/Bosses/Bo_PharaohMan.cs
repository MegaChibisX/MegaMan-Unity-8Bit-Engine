using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_PharaohMan : Boss
{

    // Animator, collider, sprite renderer and audio for the boss.
    private Animator anim;
    private BoxCollider2D col;
    private SpriteRenderer spr;
    private AudioSource aud;


    // Pharaoh Man's jumps use two corners for reference.
    public Transform leftCorner;
    public Transform rightCorner;

    // The small and big shots Pharaoh Man shoots.
    public GameObject smallShot;
    public GameObject bigShot;

    // A simple hurt sprite that Pharaoh Man displays instead of his
    // animation when he is hurt.
    public Sprite hurtSprite;
    // A regular explosion that is used in Pharaoh Man's death for fanciness.
    public GameObject explosionObject;

    // Audio Pharaoh Man uses.
    public AudioClip shotSound;
    public AudioClip chargeSound;

    // These variables are only meant to be used by Jump(bool jumpBack, bool shoot, int hops, int maxHops).
    // However, placed here, they can be viewed as Gizmos for debug purposes.
    private Vector3 targetPositionJump;
    private Vector3[] targetPositionJumpAr;


    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_PharaohMan)
            Destroy(gameObject);

        // Warns the developer if some component is missing from the boss gameObject.
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogWarning("Pharaoh Man named " + name + "has no Animator!");
        col = GetComponentInChildren<BoxCollider2D>();
        if (col == null)
            Debug.LogWarning("Pharaoh Man named " + name + "has no BoxCollider2D!");
        spr = GetComponentInChildren<SpriteRenderer>();
        if (spr == null)
            Debug.LogWarning("Pharaoh Man named " + name + "has no Sprite Renderer!");
        aud = GetComponentInChildren<AudioSource>();
        if (aud== null)
            Debug.LogWarning("Pharaoh Man named " + name + "has no Audio Source!");

        // The boss starts frozen, so they need to be kinematic.
        body = GetComponent<Rigidbody2D>();
        body.isKinematic = true;

        // The boss needs both corners to be assigned to correctly calculate the jumps.
        if (leftCorner == null || rightCorner == null || leftCorner == rightCorner)
        {
            Debug.LogError("A corner transform has not been assigned, or both have been assigned to the same gameObject.");
        }

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
            if (targetPositionJumpAr != null)
            {
                Gizmos.color = Color.green;
                foreach (Vector3 v in targetPositionJumpAr)
                    Gizmos.DrawSphere(v, 6.0f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(targetPositionJump, 8.0f);
            }
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



    public IEnumerator Behavior()
    {
        // Pharaoh Man only uses two moves in specific patterns. He jumps and he shoots.
        yield return Jump(true, true, 1, 1);
        while (true)
        {
            int roll = Random.Range(0, 4);
            switch (roll)
            {
                case 0:
                    float startRightDir = (GameManager.playerPosition.x - transform.position.x);
                    yield return Jump(false, true, 2, 4);
                    yield return Jump((GameManager.playerPosition.x - transform.position.x) * startRightDir < 0, false, 2, 2);
                    yield return new WaitForSeconds(0.4f);
                    yield return Jump(false, true, 1, 4);
                    yield return Charge();
                    yield return Jump(true, false, 1, 1);
                    break;
                case 1:
                    yield return Charge();
                    yield return Jump(false, false, 2, 4);
                    yield return Charge();
                    yield return Jump(true, true, 2, 2);
                    yield return Charge();
                    break;
                case 2:
                    yield return Jump(false, true, 1, 1);
                    yield return Charge();
                    yield return Jump(false, false, 1, 1);
                    yield return Jump(false, true, 2, 2);
                    yield return Charge();
                    break;
                case 3:
                    yield return Jump(false, false, 3, 4);
                    yield return Charge();
                    yield return Jump(true, true, 1, 1);
                    break;
            }
            yield return Jump(true, false, 1, 1);

            yield return new WaitForSeconds(0.4f);
        }
    }

    public override void Kill(bool makeItem)
    {
        // Pharaoh Man stops doing what he's doing and dies.
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

        if (Player.instance != null)
            Player.instance.CanMove(true);


        yield return new WaitForSeconds(0.4f);

        // Starts battle.
        canBeHit = true;
        StartCoroutine(Behavior());
    }

    public IEnumerator PlayDeathLong()
    {
        // Can't die if Pharaoh Man is not in a fight.
        if (!fightStarted)
            yield break;

        
        // Registers death to GameManager.
        GameManager.bossesActive--;
        GameManager.bossDead_PharaohMan = true;
        // Plays death animation.
        invisTime = 0.0f;
        anim.Play("Death");
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
            if (GameManager.bossesActive > 0)
            {
                Player.instance.CanMove(true);
                Player.instance.canBeHurt = true;
            }
            else
            {
                Player.instance.Outro();
            }
        }

        Destroy(gameObject);
    }

    public IEnumerator Jump(bool jumpBack, bool shoot, int hops, int maxHops)
    {
        yield return null;

        // Finds the farthest corner Pharaoh Man can jump to.
        Transform jumpTarget = rightCorner;
        if ((GameManager.playerPosition.x < transform.position.x) ^ jumpBack)
            jumpTarget = leftCorner;

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

            anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);

            if (jumpBack)
                anim.Play("JumpBack");
            else
                anim.Play("JumpForw");

            // The left and right corners don't account for heightened platforms in the arena,
            // so we find the actual position by raycasting from above the expected position.
            Vector2 targetPosition = new Vector3(transform.position.x + distancePerHop, jumpTarget.position.y);
            RaycastHit2D hit;

            if (hit = Physics2D.Raycast(targetPosition + Vector2.up * 128.0f, Vector2.down, 250.0f, 1 << 8))
                targetPosition.y = hit.point.y;
            targetPositionJump = targetPosition;


            // We use the pretty handy LaunchVelocity to find the trajectory of Pharaoh Man for each jump.
            body.velocity = Helper.LaunchVelocity(transform.position + (Vector3)col.offset,
                                                  (Vector2)targetPositionJumpAr[i - 1] + Vector2.up * (col.size.y),
                                                  68.0f + 3f * hops + Mathf.Max(0f, (targetPosition.y - transform.position.y)) * 0.1f,
                                                  Physics2D.gravity.y * body.gravityScale);

            // Waits for Pharaoh Man to reach the peak of his jump, then shoots
            // if needed.
            yield return null;
            while (body.velocity.y >= 0)
                yield return null;

            // Shoots if needed.
            if (shoot)
            {
                if (jumpBack)
                    anim.Play("JumpBackShoot");
                else
                    anim.Play("JumpForwShoot");

                anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);


                GameObject shot = Instantiate(smallShot);
                shot.transform.position = transform.position;
                shot.GetComponent<Rigidbody2D>().velocity = (GameManager.playerPosition - transform.position).normalized * 400.0f;

                aud.PlaySound(this.shotSound, true);
            }

            // Waits for Pharaoh Man to hit the ground.
            while (!isGrounded)
                yield return null;

            // Stands to recover after a hop.
            anim.Play("Stand");
            body.velocity = Vector2.zero;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator Charge()
    {
        yield return null;

        // Pharaoh Man looks at the player.
        anim.transform.localScale = new Vector3((GameManager.playerPosition.x < transform.position.x) ? 1 : -1, 1, 1);

        // Plays animation.
        anim.Play("Charge", -1, 0.0f);
        aud.PlaySound(chargeSound, true);

        yield return new WaitForSeconds(0.6f);

        // Pharaoh Man creates two shots, one facing forward and one to his back.
        GameObject shot = Instantiate(bigShot);
        shot.transform.position = transform.position;
        shot.GetComponent<Rigidbody2D>().velocity = Vector3.right * 300.0f;

        shot = Instantiate(bigShot);
        shot.transform.position = transform.position;
        shot.GetComponent<Rigidbody2D>().velocity = Vector3.right * -300.0f;
        shot.transform.localScale = new Vector3(-1, 1, 1);

        // Pharaoh Man stands and recovers.
        aud.PlaySound(shotSound, true);
        anim.Play("Stand");
    }


    private bool isGrounded
    {
        // Checks if Pharaoh Man is touching the ground.
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.up, new Vector2(col.size.x * 0.9f, col.size.y), 0, 1 << 8);
        }
    }


}
