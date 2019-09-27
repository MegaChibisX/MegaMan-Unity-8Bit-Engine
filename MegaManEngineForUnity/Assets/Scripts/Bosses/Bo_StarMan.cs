using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_StarMan : Boss
{

    protected Animator anim;
    protected SpriteRenderer rend;
    protected AudioSource aud;

    public BoxCollider2D col;

    public Sprite hurtSprite;

    public AudioClip shine;
    public AudioClip healSound;

    public GameObject starBulletNormal;
    public GameObject starBulletIce;

    public GameObject starShield;

    public GameObject shootingStarNormal;
    public GameObject shootingStarIce;

    public Transform cornerLeft;
    public Transform cornerRight;


    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_StarMan && !ignorePreviousDeath)
            Destroy(gameObject);

        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        anim.gameObject.SetActive(false);
    }
	private void LateUpdate()
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

        if (weapon.weaponType == Pl_Weapon.WeaponTypes.Gemini)
            damage *= 5;

        Damage(damage, weapon.ignoreInvis);
    }
    public override void Kill(bool makeItem, bool makeBolt)
    {
        StopAllCoroutines();

        // Registers death to GameManager.
        if (!ignorePreviousDeath)
            GameManager.bossDead_StarMan = true;
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
        yield return new WaitForSeconds(1f);

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
        anim.Play("Stand");
        while (true)
        {
            yield return null;

            if (Random.Range(0, 4) < 3)
            {
                yield return JumpAcrossRoom(true);
                yield return ShootAtDir(GameManager.playerPosition - transform.position, starBulletNormal);
                yield return JumpAcrossRoom(false);
                yield return ShootAtDir(GameManager.playerPosition - transform.position, starBulletIce);
            }

            yield return StarShield(3);

            if (health < 22)
            {
                yield return ShootingStar((22 - Mathf.CeilToInt(health)) / 3 + 3, 0.5f);

                if (Random.Range(0,4) < 1)
                    yield return StarShield(Random.Range(4, 7));
            }
        }
    }




    public IEnumerator JumpAcrossRoom(bool leftSide)
    {
        Vector3 jumpPos;

        if (leftSide)
            jumpPos = cornerLeft.position;
        else
            jumpPos = cornerRight.position;

        anim.Play("Jump");
        if (jumpPos.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);

        body.velocity = Helper.LaunchVelocity(transform.position, jumpPos, 50.0f, 1000f);

        while (!isGrounded)
            yield return null;

        anim.Play("Stand");
        yield return new WaitForSeconds(0.4f);
    }
    public IEnumerator ShootAtDir(Vector2 direction, GameObject shot)
    {
        if (isGrounded)
            anim.Play("StandForw");
        else
            anim.Play("JumpForw");

        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector2.zero;

        transform.localScale = new Vector3(direction.x > 0 ? -1 : 1, 1, 1);

        yield return new WaitForSeconds(0.5f);

        Shoot(transform.position + new Vector3(10 * -transform.localScale.x, 0), direction, 2, 200, shot);

        yield return new WaitForSeconds(0.1f);

        if (isGrounded)
            anim.Play("Stand");
        else
            anim.Play("Jump");

        yield return new WaitForSeconds(0.2f);

        body.bodyType = RigidbodyType2D.Dynamic;
    }
    public IEnumerator StarShield(int jumps)
    {
        anim.Play("StandUp");

        yield return new WaitForSeconds(0.5f);

        GameObject shield = Instantiate(starShield);
        shield.transform.position = transform.position;
        shield.GetComponent<Misc_FollowTransform>().target = transform;
        shield.GetComponent<Rigidbody2D>().AddTorque(5, ForceMode2D.Impulse);

        aud.PlaySound(shine, true);

        yield return new WaitForSeconds(0.2f);

        Vector3 jumpTarget;

        for (int i = 0; i < jumps - 1; i++)
        {
            jumpTarget = transform.position + (GameManager.playerPosition.x > transform.position.x ? Vector3.right : Vector3.left) * 64;
            transform.localScale = new Vector3(jumpTarget.x > transform.position.x ? -1 : 1, 1);
            body.velocity = Helper.LaunchVelocity(transform.position, jumpTarget, 83, 1000);
            anim.Play("Jump");

            while (!isGrounded)
                yield return null;

            anim.Play("Stand");
            yield return new WaitForSeconds(0.5f);
        }

        jumpTarget = transform.position + (GameManager.playerPosition.x > transform.position.x ? Vector3.right : Vector3.left) * 48;
        transform.localScale = new Vector3(jumpTarget.x > transform.position.x ? -1 : 1, 1);
        body.velocity = Helper.LaunchVelocity(transform.position, jumpTarget, 83, 1000);
        anim.Play("Jump");

        while (body.velocity.y > 0)
            yield return null;

        body.gravityScale = 0f;
        body.velocity = Vector2.zero;

        anim.Play("JumpUp");

        yield return new WaitForSeconds(0.3f);

        shield.GetComponent<Rigidbody2D>().velocity = (GameManager.playerPosition - transform.position).normalized * 150f;
        shield.GetComponent<Misc_FollowTransform>().target = null;
        aud.PlaySound(shine, true);

        yield return new WaitForSeconds(0.3f);
        body.gravityScale = 100f;

        while (!isGrounded)
            yield return null;

        anim.Play("Stand");
        yield return new WaitForSeconds(0.3f);
    }
    public IEnumerator ShootingStar(int stars, float dropDelay)
    {
        while (!isGrounded)
            yield return null;

        anim.Play("StandUp");
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < stars; i++)
        {
            Shoot(transform.position + new Vector3(-transform.localScale.x * 9, 15), Vector2.up, 1, 500, (i % 3 == 2 ? starBulletIce : starBulletNormal));
            yield return new WaitForSeconds(0.07f);
        }

        yield return new WaitForSeconds(dropDelay);

        StartCoroutine(DropStars(stars));
    }
    public IEnumerator DropStars(int stars)
    {
        if (stars <= 0)
            stars = 1;

        for (int i = 0; i < stars; i++)
        {
            GameObject star = Instantiate((i % 3 == 2 ? shootingStarIce : shootingStarNormal));
            star.transform.position = GameManager.playerPosition + Vector3.up * 240;
            GameManager.ShakeCamera(0.2f, 3f,  false);

            yield return new WaitForSeconds(0.5f);
        }
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
