using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_GalaxyMan: Boss
{

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;

    public Transform upHeight;
    public Transform downHeight;

    public EnWp_TrackBullet blackHoleBaby;

    public AudioClip healSound;

    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_GalaxyMan && !ignorePreviousDeath)
            Destroy(gameObject);

        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        anim.gameObject.SetActive(false);
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
            GameManager.bossDead_GalaxyMan = true;
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
        StartCoroutine(Behavior());
    }

    public IEnumerator Behavior()
    {
        while (true)
        {
            yield return null;

            LookAtPlayer();

            yield return new WaitForSeconds(0.2f);

            yield return Fly(true);
            yield return FlyAround(1.0f);

            yield return Shoot();

            anim.Play("Stand");
        }
    }

    public IEnumerator Fly(bool up)
    {
        Vector3 targetPos = new Vector3(transform.position.x, 0, transform.position.z);
        if (up)
            targetPos.y = upHeight.position.y; 
        else
            targetPos.y = downHeight.position.y;

        anim.Play("Fly");

        while ((targetPos - transform.position).sqrMagnitude > 1)
        {
            body.position = Vector3.MoveTowards(body.position, targetPos, 128f * Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator FlyAround(float time)
    {
        anim.Play("Fly");
        float height = upHeight.position.y;

        while (time > 0)
        {
            Vector3 targetPos = transform.position - transform.right * transform.localScale.x * 128f * Time.fixedDeltaTime;
            targetPos.y += Mathf.Sin(Time.time * 360f * Mathf.Deg2Rad) * 2 * Time.timeScale;
            body.position = targetPos;

            if (isWalled)
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

            time -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    public IEnumerator Shoot()
    {
        Enemy hole = Instantiate(blackHoleBaby.gameObject).GetComponent<Enemy>();
        hole.transform.position = transform.position;

        float targetHeight = downHeight.position.y + 54;
        while (hole.transform.position.y > targetHeight)
        {
            yield return FlyAround(0.1f);
        }

        anim.Play("Disappear");
        yield return new WaitForSeconds(0.3f);

        transform.position = new Vector3(hole.transform.position.x, downHeight.position.y, transform.position.z);
        hole.Kill(false, false);

        anim.Play("Show");
        yield return new WaitForSeconds(0.75f);

        anim.Play("Pose");
        yield return new WaitForSeconds(4.25f);
    }


    private bool isGrounded
    {
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.up, new Vector2(col.size.x * 0.9f, col.size.y), 0, 1 << 8);
        }
    }
    private bool isWalled
    {
        get
        {
            return 
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.right * transform.localScale.x * col.size.y * 0.5f,
                                            new Vector2(col.size.x, col.size.y * 0.45f), 0, 1 << 8);
        }
    }


}
