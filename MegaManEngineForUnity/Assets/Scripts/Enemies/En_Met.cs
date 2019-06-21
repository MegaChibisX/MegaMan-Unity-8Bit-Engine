using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the Mets use a single script, as they have many common features.
/// The Met acts differently based on the 
/// </summary>
[AddComponentMenu("MegaMan/Enemy/Met")]
public class En_Met : Enemy
{

    public enum MetBehaviors { Idle, Walking, Helicopter, Dance, Swim, Space, Mommy, K1000, Cannon }
    public MetBehaviors behavior;

    public float moveSpeed = 100.0f;
    public float flySpeed = 50.0f;
    public float swimSpeed = 100.0f;
    public float chargeSpeed = 200.0f;

    // The time the Met stays hidden after each cycle.
    protected float waitDownTime = 2.0f;
    // The time the Met stays hidden when it spawns.
    public float startWaitTime = 3.0f;
    // The time the Met stays open after an attack.
    public float waitTimeAfterAttack = 2.0f;

    public EnWp_Shot shot;

    private Animator anim;


    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();

        // Some Mets have unique guard animations that need to be played
        // the first time the Met appears.
        switch (behavior)
        {
            default:
                anim.Play("MetDown");
                break;
            case MetBehaviors.K1000:
                anim.Play("MetTrainDown");
                break;
            case MetBehaviors.Space:
                anim.Play("MetSpaceIdle");
                break;
            case MetBehaviors.Cannon:
                anim.Play("MetCannonIdle");
                break;
        }
        // Sets the Met state.
        waitDownTime = startWaitTime;
        shielded = true;
        LookAtPlayer();
    }
    protected void Update()
    {
        // Does nothing if MegaMan hasn't fully entered the room or fully spawned.
        if (!GameManager.roomFinishedLoading)
            return;

        // All the mets stay down if they are guarding.
        // After that, each does its unique thing.
        if (waitDownTime > 0.0f)
        {
            waitDownTime -= Time.deltaTime;
            if (waitDownTime <= 0.0f)
            {
                switch (behavior)
                {
                    case MetBehaviors.Idle:
                        StartCoroutine(MetIdle(0.75f, 3, 3));
                        break;
                    case MetBehaviors.Walking:
                        StartCoroutine(MetWalk(0.75f, 3, 3));
                        break;
                    case MetBehaviors.Helicopter:
                        StartCoroutine(MetHeli(0.75f, 1, 3));
                        break;
                    case MetBehaviors.Dance:
                        StartCoroutine(MetDance(0.5f, 3));
                        break;
                    case MetBehaviors.Swim:
                        StartCoroutine(MetSwim(0.5f, 3, 3));
                        break;
                    case MetBehaviors.Space:
                        StartCoroutine(MetSpace(1.0f, 2.0f, 4));
                        break;
                    case MetBehaviors.Mommy:
                        StartCoroutine(MetWalk(0.5f, 3, 3));
                        break;
                    case MetBehaviors.K1000:
                        StartCoroutine(MetK1000(1.0f, 0.25f, 1, 3));
                        break;
                    case MetBehaviors.Cannon:
                        StartCoroutine(MetCannon());
                        break;
                }
            }
        }
    }
    protected bool isGrounded
    {
        // Checks if the Met is touching the ground.
        get
        {
            return Physics2D.Linecast(transform.position, transform.position - transform.up * 5.0f, 1 << 8);
        }
    }


    protected IEnumerator MetIdle(float timePerLoop, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetIdle");
        yield return new WaitForSeconds(0.2f);

        // Looks at the player.
        LookAtPlayer();

        // Each loop is a row of shots.
        for (int i = 0; i < loops; i++)
        {
            // Finds the higher/lower shot in a row, and goes to the next shot in each iteration.
            Vector2 shotDir = -transform.right;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // Waits until the next row of shots need to be fired.
            float time = timePerLoop;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                yield return null;
            }
        }

        // Shields the Met.
        body.velocity = Vector2.zero;
        anim.Play("MetShielded");
        waitDownTime = waitTimeAfterAttack;
        shielded = true;

        LookAtPlayer();
    }
    protected IEnumerator MetWalk(float timePerLoop, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetUp");
        yield return new WaitForSeconds(0.2f);

        // Looks at the player and starts walking.
        anim.Play("MetWalk");
        LookAtPlayer();

        for (int i = 0; i < loops; i++)
        {
            // Just like MetIdle.
            Vector2 shotDir = -transform.right;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // While waiting between each row, the Met walks instead of standing still.
            float time = timePerLoop;
            while (time > 0.0f)
            {
                body.velocity = -transform.right * moveSpeed + Vector3.up * body.velocity.y;
                time -= Time.deltaTime;
                yield return null;
            }
        }

        body.velocity = Vector2.zero;
        anim.Play("MetDown");
        waitDownTime = waitTimeAfterAttack;
        shielded = true;

        LookAtPlayer();
    }
    protected IEnumerator MetHeli(float timePerLoop, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetHeliUp");
        yield return new WaitForSeconds(0.5f);
        LookAtPlayer();

        anim.Play("MetHeliFly");
        body.gravityScale = 0.0f;

        // Finds how high the enemy needs to go.
        float flyheight = (GameManager.playerPosition.y - transform.position.y);
        flyheight = Mathf.Clamp(flyheight, 64, 128);

        // Flies the enemy straight up untuil it has reached its desired height.
        while (flyheight > 0.0f)
        {
            body.MovePosition(transform.position + transform.up * flySpeed * Time.deltaTime);
            flyheight -= flySpeed * Time.deltaTime;
            yield return null;
        }


        // Shoots each row of shots and moves.
        for (int i = 0; i < loops; i++)
        {
            // Just like MetIdle.
            Vector2 shotDir = GameManager.playerPosition - transform.position;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // Moves forward instead of standing still between each shot.
            float time = timePerLoop;
            while (time > 0.0f)
            {
                body.velocity = -transform.right * moveSpeed + Vector3.up * body.velocity.y;
                time -= Time.deltaTime;
                yield return null;
            }
        }


        // Drops the Met and waits until it hits the ground.
        body.velocity = Vector2.zero;
        body.gravityScale = 100.0f;
        while (!isGrounded)
            yield return null;


        // Shields the Met.
        anim.Play("MetDown");
        waitDownTime = waitTimeAfterAttack;
        shielded = true;

        LookAtPlayer();
    }
    protected IEnumerator MetDance(float timePerLoop, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetDance");
        yield return new WaitForSeconds(0.2f);

        // Looks at the player.
        LookAtPlayer();

        // Goes through each row of shots.
        for (int i = 0; i < loops; i++)
        {
            // Just like MetIdle.
            Vector2 shotDir = -transform.right;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // Waits between each loop.
            float time = timePerLoop;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                yield return null;
            }
        }

        // Resets and shields the Met.
        body.velocity = Vector2.zero;
        anim.Play("MetShielded");
        waitDownTime = waitTimeAfterAttack;
        shielded = true;

        LookAtPlayer();
    }
    protected IEnumerator MetSwim(float timePerLoop, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetGogglesUp");
        yield return new WaitForSeconds(0.5f);
        LookAtPlayer();

        // Sets the Met gravity to be underwater. Place a swimming met outside water at your own risk.
        body.gravityScale = 25.0f;


        // Shoots each row of shots.
        for (int i = 0; i < loops; i++)
        {
            // If the Met is in water, plays the swimming animation.
            if (Physics2D.OverlapPoint(transform.position + center, 1 << 4))
                anim.Play("MetGogglesSwim", -1, 0.0f);

            // Just like MetIdle.
            Vector2 shotDir = -transform.right;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // If the Met is in water, it swims.
            float time = timePerLoop;
            while (time > 0.0f)
            {
                if (time > timePerLoop * 0.5f && Physics2D.OverlapPoint(transform.position + center, 1 << 4))
                {
                    body.velocity = transform.up * flySpeed;
                }
                time -= Time.deltaTime;
                yield return null;
            }
        }


        // Sets the gravity and waits to touch the ground.
        body.gravityScale = 25.0f;
        while (!isGrounded)
            yield return null;


        // Resets and guards the Met.
        anim.Play("MetDown");
        waitDownTime = waitTimeAfterAttack;
        shielded = true;

        LookAtPlayer();
    }
    protected IEnumerator MetSpace(float timePerLoop, float waitAfterTime, int loops)
    {
        // Makes the Met damagable.
        shielded = false;
        body.gravityScale = 0.0f;

        anim.Play("MetSpaceFloat");

        // The Space Met doesn't shoot, but it dashes at the player instead.
        // Dashes a few times, with a small delay each time.
        for (int i = 0; i < loops; i++)
        {
            float time = timePerLoop;
            Vector2 dir = (GameManager.playerPosition - transform.position).normalized;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                body.velocity = dir * flySpeed;
                yield return null;
            }

            body.velocity = Vector3.zero;
            yield return new WaitForSeconds(0.25f);
        }

        // Resets the Met and guards it.
        body.gravityScale = 100.0f;

        anim.Play("MetSpaceIdle");
        waitDownTime = waitAfterTime;
    }
    protected IEnumerator MetK1000(float timePerLoop, float waitAfterTime, int loops, int ShotNumber = 1, float shotAngle = 45.0f)
    {
        // Looks at the player.
        LookAtPlayer();

        // Moves slowly while guarded.
        float walkTime = timePerLoop * loops;
        Vector2 dir = -transform.right;
        while (walkTime > 0.0f)
        {
            walkTime -= Time.deltaTime;
            body.velocity = dir * moveSpeed + Vector2.up * body.velocity.y;
            yield return null;
        }

        // Makes the Met damagable.
        shielded = false;
        anim.Play("MetTrainUp");

        // Just like MetIdle.
        for (int i = 0; i < loops; i++)
        {
            Vector2 shotDir = -transform.right;
            shotDir = Quaternion.AngleAxis((ShotNumber - 1) * shotAngle * -0.5f, Vector3.forward) * shotDir;
            for (int q = 0; q < ShotNumber; q++)
            {
                Shoot(transform.position + center - transform.right * 8.0f, shotDir, 2);
                shotDir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * shotDir;
            }

            // Moves forward faster.
            while (timePerLoop > 0.0f)
            {
                timePerLoop -= Time.deltaTime;
                body.velocity = dir * chargeSpeed + Vector2.up * body.velocity.y;
                yield return null;
            }
        }

        // Resets and shields the Met.
        shielded = true;
        body.velocity = Vector3.zero;

        // Waits.
        anim.Play("MetTrainDown");
        waitDownTime = waitAfterTime;
    }
    protected IEnumerator MetCannon()
    {
        do
        {
            yield return null;
        } while (Mathf.Abs(GameManager.playerPosition.x - transform.position.x) > 80.0f);

        anim.Play("MetCannonShielded");

        yield return new WaitForSeconds(0.2f);

        // Waits for the player to get close in the X axis.
        while (Mathf.Abs(GameManager.playerPosition.x - transform.position.x) <= 112.0f)
        {

            // Turns to face the player.
            if ((GameManager.playerPosition.x - transform.position.x) * transform.right.x > 0)
            {
                anim.Play("MetCannonTurn");
                yield return new WaitForSeconds(0.1f);
                anim.Play("MetCannonShielded");
                LookAtPlayer();
            }

            // Waits for the player to come in line of the cannon to shoot.
            if (Mathf.Abs(GameManager.playerPosition.y - transform.position.y) < 16)
            {
                anim.Play("MetCannonAwake");
                shielded = false;
                yield return new WaitForSeconds(0.1f);
                anim.Play("MetCannonShoot");
                    
                // Shoots a big shot.
                EnWp_Shot shot = ((GameObject)Instantiate(Resources.Load("Prefabs/Enemies/En_BigShot1"))).GetComponent<EnWp_Shot>();
                shot.transform.position = transform.position + center + Vector3.forward;
                shot.direction = transform.right * (transform.localScale.x > 0 ? -1 : 1);
                shot.speed = 150.0f;

                yield return new WaitForSeconds(0.75f);
                break;
            }

            yield return null;
        }

        // Resets and shields the Met.
        shielded = true;
        anim.Play("MetCannonIdle");
        waitDownTime = 1.0f;


    }



}
