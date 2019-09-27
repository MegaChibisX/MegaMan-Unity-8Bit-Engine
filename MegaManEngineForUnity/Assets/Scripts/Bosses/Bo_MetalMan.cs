using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_MetalMan : Boss
{

    [Header("--------")]
    public Transform leftSide;
    public Transform rightSide;

    public Sprite hurtSprite;
    public AudioClip healSound;

    public GameObject metalBlade;

    protected Animator anim;
    protected SpriteRenderer spr;
    protected AudioSource aud;

    public Collider2D col;
    public GameObject convLeft;
    public GameObject convRight;



    protected override void Start()
    {
        base.Start();

        // Destroy Metal Man if he has already been beaten.
        if (GameManager.bossDead_MetalMan && !ignorePreviousDeath)
            Destroy(transform.parent.gameObject);

        anim = GetComponentInChildren<Animator>();
        spr = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        anim.gameObject.SetActive(false);
        SetConveyor(true);
    }
    private void LateUpdate()
    {
        // If invisible, makes the boss flash.
        if (invisTime % 0.2f > 0.07f)
            spr.sprite = hurtSprite;
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }

    public override void StartFight()
    {
        base.StartFight();
        anim.gameObject.SetActive(true);
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
            Kill(false, false);
        }
    }
    public override void Kill(bool makeItem, bool makeBolt)
    {
        // Metal Man stops doing what he's doing and dies.
        StopAllCoroutines();
        if (!ignorePreviousDeath)
            GameManager.bossDead_MetalMan = true;
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.PlayMusic(null);
    }
    private void SetConveyor(bool right)
    {
        if (convRight)
            convRight.SetActive(right);
        if (convLeft)
            convLeft.SetActive(!right);
    }


    public IEnumerator Behavior()
    {
        yield return null;

        while (true)
        {
            yield return null;

            if (Random.Range(0, 10) == 0 && convRight != null)
                SetConveyor(!convRight.activeSelf);

            anim.transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? -1 : 1, 1, 1);

            if (Mathf.Abs(GameManager.playerPosition.x - transform.position.x) < 80)
                yield return JumpAcross();

            anim.transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? -1 : 1, 1, 1);
            yield return JumpThrow();
        }
    }
    public IEnumerator JumpThrow()
    {
        float jumpHeight = (300f + Mathf.Min(200f, Mathf.Abs(transform.position.y - GameManager.playerPosition.y) * 3));
        body.velocity = transform.up * jumpHeight;

        anim.Play("JumpHold");

        while (body.velocity.y > 0)
            yield return null;

        anim.Play("JumpThrow");


        int jumps = (int)((jumpHeight - 300) / 75) + 1;
        for (int i = 0; i < jumps; i++)
        {
            ThrowBlade();
            body.velocity = Vector3.zero;

            float time = 0.25f;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                if (isGrounded)
                    break;

                yield return null;
            }
        }

        while (!isGrounded)
            yield return null;

        anim.transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? -1 : 1, 1, 1);
        anim.Play("Run");
        yield return new WaitForSeconds(0.1f + 0.1f * jumps);

    }
    public IEnumerator JumpAcross()
    {
        yield return null;


        Vector3 jumpTarget = rightSide.position;
        if (Vector3.Distance(transform.position, leftSide.position) > Vector3.Distance(transform.position, rightSide.position))
            jumpTarget = leftSide.position;

        Vector3 velocity = Helper.LaunchVelocity(transform.position, jumpTarget, 60, Physics2D.gravity.y * body.gravityScale);
        body.velocity = velocity;

        anim.Play("Jump");

        while (body.velocity.y > 1.0f || !isGrounded)
            yield return null;

        if (convRight != null)
            SetConveyor(!convRight.activeSelf);

        body.velocity = Vector3.zero;

        yield return null;
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

        yield return null;
        anim.Play("Intro");

        yield return new WaitForSeconds(2.0f);

        // Slowly gains health.
        for (int i = 0; i < 28; i++)
        {
            health++;
            yield return new WaitForSeconds(0.05f);
            Helper.PlaySound(aud, healSound, true);
        }


        anim.Play("Idle");

        yield return null;

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
        GameManager.bossDead_MetalMan = true;
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
            Player.instance.StopAllCoroutines();
            Player.instance.body.velocity = Vector2.zero;
            Player.instance.CanMove(false);
            Player.instance.SetGear(false, false);
            Player.instance.RefreshWeaponList();
            Player.instance.canBeHurt = false;
        }

        yield return new WaitForSecondsRealtime(1.0f);

        // Explodes.
        for (int i = 0; i < 10; i++)
        {
            GameObject exp = Instantiate(deathExplosion);
            exp.transform.position = transform.position + new Vector3(Random.Range(-64f, 64f), Random.Range(-64f, 64f), -10f);
            yield return new WaitForSecondsRealtime(0.3f);
        }

        // Explodes more.
        for (int i = 0; i < 5; i++)
        {
            GameObject exp = Instantiate(deathExplosion);
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
            if (GameManager.bossesActive > 0 || !endStageAfterFight)
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


    public void ThrowBlade()
    {
        Shoot(transform.position, GameManager.playerPosition - transform.position, 1f, 300, metalBlade);
    }


    public bool isGrounded
    {
        get
        {
            // Layer 8 == Solid layer
            return Physics2D.OverlapBox(col.bounds.center + Vector3.down * 2.0f, col.bounds.size, 0f, 1 << 8);
        }
    }


}
