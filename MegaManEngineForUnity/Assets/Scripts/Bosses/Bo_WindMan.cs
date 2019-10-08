using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_WindMan : Boss
{

    private Animator anim;
    private AudioSource aud;

    public BoxCollider2D col;
    public ParticleSystem particles;

    public AudioClip healSound;

    public EnWp_WaveShot shot;
    public Stage_WindZone wind;

    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_WindMan && !ignorePreviousDeath)
            Destroy(gameObject);

        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        anim.gameObject.SetActive(false);
        wind.gameObject.SetActive(false);
    }
    protected override void LateUpdate()
    {
        if (invisTime % 0.2f > 0.07f)
            rend.sprite = hurtSprite;
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }

    public override void Damage(Pl_Weapon weapon)
    {
        float damage = weapon.damage;

        if (shielded && !weapon.ignoreShield)
        {
            weapon.Deflect();
            return;
        }

        Damage(damage, weapon.ignoreInvis);
    }
    public override void Kill(bool makeItem, bool makeBolt)
    {
        StopAllCoroutines();

        // Registers death to GameManager.
        if (!ignorePreviousDeath)
            GameManager.bossDead_WindMan = true;
        particles.Stop();
        Destroy(wind.gameObject);
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.PlayMusic(null);
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
        anim.gameObject.SetActive(true);
        anim.Play("Intro");
        yield return new WaitForSeconds(1.5f);

        // Slowly gains health.
        for (int i = 0; i < 28; i++)
        {
            health++;
            yield return new WaitForSeconds(0.05f);
            Helper.PlaySound(aud, healSound, true);
        }

        // Unfreezes player and self.
        body.isKinematic = false;

        if (Player.instance != null)
            Player.instance.CanMove(true);


        yield return new WaitForSeconds(0.4f);

        // Starts battle.
        canBeHit = true;
        wind.gameObject.SetActive(true);
        StartCoroutine(Behavior());
    }
    private IEnumerator Behavior()
    {
        while (true)
        {
            yield return null;


            LookAtPlayer();
            anim.Play("Windy");

            shielded = true;
            yield return new WaitForSeconds(0.5f);
            wind.direction = new Vector2(transform.localScale.x, 0);
            yield return new WaitForSeconds(0.5f);
            shielded = false;


            for (int i = 0; i < Random.Range(1, 3); i++)
            {
                yield return FlyAtPlayer();

                anim.Play("Stand");
                yield return new WaitForSeconds(0.25f);
            }

            for (int i = 0; i < Random.Range(0, 3); i++)
            {
                LookAtPlayer();
                yield return Shoot();

                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator FlyAtPlayer()
    {
        anim.Play("Fly");
        LookAtPlayer();
        particles.Play();

        Vector3 targetPos = transform.position + Vector3.up * 64;
        Vector2 prevPos = body.position;

        while ((targetPos - transform.position).sqrMagnitude > 1f)
        {
            body.position = Vector3.MoveTowards(body.position, targetPos, Time.fixedDeltaTime * 256f);

            if (prevPos == body.position)
                break;
            prevPos = body.position;

            yield return new WaitForFixedUpdate();
        }

        targetPos = GameManager.playerPosition;
        targetPos.y = transform.position.y;
        while ((targetPos - transform.position).sqrMagnitude > 1f)
        {
            body.position = Vector3.MoveTowards(body.position, targetPos, Time.fixedDeltaTime * 256f);

            if (prevPos == body.position)
                break;
            prevPos = body.position;

            yield return new WaitForFixedUpdate();
        }


        while (!isGrounded)
        {
            body.MovePosition(body.position + Vector2.down * 256f * Time.fixedDeltaTime);

            //if (prevPos == body.position)
            //    break;
            //prevPos = body.position;

            yield return new WaitForFixedUpdate();
        }

        particles.Stop();
    }
    private IEnumerator Shoot()
    {
        anim.Play("Shoot");

        yield return new WaitForSeconds(0.25f);

        EnWp_WaveShot newShot = Instantiate(shot.gameObject).GetComponent<EnWp_WaveShot>();
        newShot.transform.position = transform.position;
        newShot.transform.localScale = new Vector3(-transform.localScale.x * transform.right.x, transform.localScale.y, transform.localScale.z);

        newShot = Instantiate(shot.gameObject).GetComponent<EnWp_WaveShot>();
        newShot.transform.position = transform.position;
        newShot.transform.localScale = new Vector3(-transform.localScale.x * transform.right.x, transform.localScale.y, transform.localScale.z);
        newShot.waveLength *= -1f;
    }


    private bool isGrounded
    {
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.up, new Vector2(col.size.x * 0.9f, col.size.y), 0, 1 << 8);
        }
    }


}
