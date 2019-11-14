using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_BombMan : Boss
{

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;


    public AudioClip healSound;

    private string curAnim;

    [Header("------")]
    public EnWp_Bomb bombClassic;
    public EnWp_Bomb bombCross;
    public EnWp_Bomb bombX;
    public EnWp_Bomb bombFlower;
    public EnWp_Bomb bombRubber;

    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        if (GameManager.bossDead_BombMan && !ignorePreviousDeath)
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
            case Pl_Weapon.WeaponTypes.Pharaoh:
                damage *=2 ;
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
            GameManager.bossDead_BombMan = true;
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.aud.Stop();
    }
    private void AnimPlay(string animName)
    {
        anim.Play(animName);
        curAnim = animName;
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

    private IEnumerator Behavior()
    {
        while (true)
        {
            yield return null;

            int roll = Random.Range(0, 9);
            float cooldown = 0.0f;

            LookAtPlayer();

            switch (roll)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    yield return Jump(96);

                    StartCoroutine(Throw(GameManager.playerPosition, bombClassic, 60));

                    while (!isGrounded)
                        yield return null;
                    body.velocity = Vector2.zero;
                    AnimPlay("Stand");
                    break;
                case 4:
                case 5:

                    StartCoroutine(Jump(0, 60));

                    cooldown = 0.0f;
                    while (body.velocity.y > 0.0f || !isGrounded)
                    {
                        cooldown -= Time.deltaTime;
                        if (cooldown <= 0.0f)
                        {
                            EnWp_Bomb bomb = Random.Range(0, 2) == 0 ? bombCross : bombX;
                            StartCoroutine(Throw(GameManager.playerPosition + Vector3.right * Random.Range(-32, 32), bomb));
                            cooldown = 0.25f;
                        }

                        yield return null;
                    }
                    body.velocity = Vector2.zero;
                    AnimPlay("Stand");
                    break;
                case 6:
                    StartCoroutine(Jump(-64, 82));

                    cooldown = 0.0f;
                    while (body.velocity.y > 0.0f || !isGrounded)
                    {
                        cooldown -= Time.deltaTime;
                        if (cooldown <= 0.0f)
                        {
                            StartCoroutine(Throw(GameManager.playerPosition + Vector3.right * Random.Range(-96, 96), bombRubber));
                            cooldown = 0.5f;
                        }

                        yield return null;
                    }
                    body.velocity = Vector2.zero;
                    AnimPlay("Stand");
                    yield return new WaitForSeconds(0.5f);

                    break;
                case 7:

                    for  (int i = 0;  i < 6; i++)
                    {
                        yield return Throw(transform.position + transform.up * 16f, bombClassic);
                        health += 0.5f;
                        aud.PlaySound(healSound, true);
                    }
                    break;
                case 8:
                    yield return new WaitForSeconds(0.5f);

                    yield return Throw(GameManager.playerPosition, bombFlower, 70);

                    yield return Jump(0, 80f);
                    while (!isGrounded)
                        yield return null;
                    body.velocity = Vector2.zero;

                    break;

            }

            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator Jump(float distance, float angle  = 75f)
    {

        Vector3 targetPos = transform.position - transform.right * distance * transform.localScale.x;
        Vector3 velocity;
        if (distance == 0)
        {
            body.velocity = transform.up * angle;
        }
        else
        {
            velocity = Helper.LaunchVelocity(transform.position, targetPos, angle, 1000);
            body.velocity = velocity;
        }


        if (distance > 0)
            AnimPlay("JumpF");
        else if (distance < 0)
            AnimPlay("JumpB");
        else
            AnimPlay("Jump");

        while (body.velocity.y > 0.0f)
            yield return null;

    }
    private IEnumerator Throw(Vector3 targetPos, EnWp_Bomb bombType, float angle = 70f)
    {   
        switch (curAnim)
        {
            default:
                anim.Play("ThrowStand");
                break;
            case "Jump":
                anim.Play("ThrowJump");
                break;
            case "JumpB":
                anim.Play("ThrowJumpB");
                break;
            case "JumpF":
                anim.Play("ThrowJumpF");
                break;
        }

        GameObject bomb = Instantiate(bombType.gameObject);
        bomb.transform.position = transform.position + transform.up * 6f;

        Vector3 velocity = Helper.LaunchVelocity(bomb.transform.position, targetPos, angle, 1000);
        bomb.GetComponent<Rigidbody2D>().velocity = velocity;

        yield return new WaitForSeconds(0.1f);

        anim.Play(curAnim);
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
