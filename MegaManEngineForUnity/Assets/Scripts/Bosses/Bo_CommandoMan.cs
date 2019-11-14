using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_CommandoMan : Boss
{

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;

    public AudioClip healSound;

    public EnWp_CommandoBomb bomb;

    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_CommandoMan && !ignorePreviousDeath)
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

        switch (weapon.weaponType)
        {
            case Pl_Weapon.WeaponTypes.Metal:
                damage *= 2;
                break;
            case Pl_Weapon.WeaponTypes.Galaxy:
                damage = 0;
                break;
            case Pl_Weapon.WeaponTypes.Gemini:
                damage = 0.3333f;
                break;
            default:
                damage = 1;
                break;
        }

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
        GameManager.bossesActive--;
        if (!ignorePreviousDeath)
            GameManager.bossDead_CommandoMan = true;
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.aud.Stop();
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
        yield return new WaitForSeconds(3f);

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

            yield return new WaitForSeconds(0.5f);

            int roll = Random.Range(0, 3);
            Vector3 prevVel;

            switch (roll) {
                case 0:
                    yield return Jump(false);
                    LookAtPlayer();

                    prevVel = body.velocity;
                    yield return Shoot();
                    body.velocity = prevVel;

                    while (!isGrounded)
                        yield return null;
                    body.velocity = Vector2.zero;
                    break;
                case 1:
                    yield return new WaitForSeconds(0.25f);
                    yield return Shoot();
                    break;
                case 2:
                    yield return Jump(true);
                    LookAtPlayer();

                    prevVel = body.velocity;
                    yield return Shoot();
                    body.velocity = prevVel;

                    yield return Drop();
                    break;
    
            }

            anim.Play("Stand");
        }
    }

    private IEnumerator Jump(bool big)
    {
        anim.Play("Jump");

        Vector3 targetPos;
        Vector3 velocity;
        if (big)
        {
            targetPos = transform.position + transform.right * -transform.localScale.x * 80;
            velocity = Helper.LaunchVelocity(transform.position, targetPos, 80, 1000);
        }
        else
        {
            targetPos = transform.position + transform.right * -transform.localScale.x * 64;
            velocity = Helper.LaunchVelocity(transform.position, targetPos, 75f, 1000);
        }
        body.velocity = velocity;

        while (body.velocity.y > 0)
            yield return null;
    }
    private IEnumerator Shoot()
    {
        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector3.zero;

        if (isGrounded)
            anim.Play("StandShoot");
        else
            anim.Play("JumpShoot");

        yield return new WaitForSeconds(0.35f);

        EnWp_CommandoBomb newBomb = Instantiate(bomb.gameObject).GetComponent<EnWp_CommandoBomb>();
        newBomb.transform.position = transform.position - transform.right * transform.localScale.x * 32;
        newBomb.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);


        newBomb.startDir = -transform.right * transform.localScale.x;

        yield return new WaitForSeconds(0.15f);
        body.bodyType = RigidbodyType2D.Dynamic;
    }
    private  IEnumerator Drop()
    {
        shielded = true;

        anim.Play("Drop");
        body.bodyType = RigidbodyType2D.Dynamic;

        while (!isGrounded)
        {
            body.velocity = -transform.up * 200f;

            yield return null;
        }

        GameManager.ShakeCamera(0.4f, 2f,  true);

        anim.Play("Dropped");
        body.velocity = Vector3.zero;
        yield return new WaitForSeconds(0.5f);

        shielded = false;
    }


    private bool isGrounded
    {
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.up, new Vector2(col.size.x * 0.85f, col.size.y), 0, 1 << 8);
        }
    }


}
