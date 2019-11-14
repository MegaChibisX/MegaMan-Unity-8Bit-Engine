using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_Roll1 : Boss
{

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;

    public AudioClip healSound;

    public Transform leftCorner;
    public Transform rightCorner;

    public GameObject thunderObj;
    public GameObject sliderObj;
    public GameObject heartShotObj;
    public GameObject charge1Obj;
    public GameObject charge2Obj;

    public Boss nextBoss;

    protected override void Start()
    {
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
        // Registers death to GameManager.
        StopAllCoroutines();
        StartCoroutine(PlayDeathShort());
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
        yield return new WaitForSeconds(2.0f);

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
    public new IEnumerator PlayDeathShort()
    {
        if (!fightStarted)
            yield break;

        fightStarted = false;
        deathExplosionBoss = Instantiate(deathExplosionBoss);
        deathExplosionBoss.transform.position = transform.position;
        if (rend)
            rend.gameObject.SetActive(false);

        if (GameManager.bossesActive == 1 && endStageAfterFight)
        {
            if (Player.instance != null)
                Player.instance.canBeHurt = false;
            yield return new WaitForSeconds(4.0f);

            Time.timeScale = 0.0f;
            if (Player.instance != null)
            {
                Player.instance.StopAllCoroutines();
                Player.instance.body.velocity = Vector2.zero;
                Player.instance.CanMove(false);
                Player.instance.SetGear(false, false);
                Player.instance.RefreshWeaponList();
            }

            yield return new WaitForSeconds(2.0f);

            Time.timeScale = 1.0f;
            if (Player.instance != null)
            {
                if (nextBoss != null)
                {
                    Player.instance.CanMove(true);
                    Player.instance.canBeHurt = true;

                    nextBoss = Instantiate(nextBoss);
                    nextBoss.transform.position = new Vector3((leftCorner.position.x + rightCorner.position.x) * 0.5f, 0f, 0f);
                    nextBoss.StartFight();
                }
                else
                {
                    Player.instance.Outro();
                }
            }
        }

        Destroy(gameObject);
    }


    public IEnumerator Behavior()
    {
        while (true)
        {
            anim.Play("Stand");
            yield return null;

            GameManager.Players curPlayer = GameManager.Players.MegaMan;
            if (Player.instance != null)
                curPlayer = Player.instance.curPlayer;

            int diceRoll = Random.Range(0, 8);
            switch (diceRoll)
            {
                // Thunder A
                case 0:
                    if (curPlayer == GameManager.Players.MegaManJet)
                        break;

                    yield return JumpToCenter();
                    yield return FixedThunder();

                    anim.Play("Stand");
                    yield return new WaitForSeconds(0.5f);
                    break;
                // Ground swipe
                case 1:
                    if (curPlayer != GameManager.Players.MegaManJet &&
                        curPlayer != GameManager.Players.Bass)
                        break;

                    yield return JetSliders(curPlayer == GameManager.Players.MegaManJet ? 2 : 1);
                    break;
                // Big Heart
                case 2:
                    yield return Earthquake();
                    yield return HeartShot();

                    yield return Jump(true, 0);
                    yield return JumpToCenter();
                    while (!isGrounded)
                        yield return null;
                    yield return Jump(true, 0);

                    break;
                case 3:
                    yield return SlidersHor(2);
                    break;
                case 4:
                    yield return SlidersVer(2);
                    break;
                case 5:
                    yield return Shoot_MegaBuster(3);
                    break;
                case 6:
                    yield return ChasePlayer(3f);
                    break;
                case 7:
                    yield return Rollnado(10);
                    yield return Dash();
                    anim.Play("Jump");
                    yield return Earthquake();
                    break;
            }

        }
    }

    public IEnumerator Jump(bool alsoLand, float hSpeed)
    {
        anim.Play("Jump");

        body.velocity = Vector3.up * 370f + Vector3.right * hSpeed;

        while (body.velocity.y > 0.0f)
            yield return null;

        if (alsoLand)
        {
            while (!isGrounded)
                yield return null;
        }

        anim.Play("Stand");
    }
    public IEnumerator JumpToCenter()
    {

        Vector3 targetPos = (rightCorner.position + leftCorner.position) * 0.5f + Vector3.up * 150f;

        body.velocity = Helper.LaunchVelocity(transform.position, targetPos,
                        70,
                        1000f);

        float prevDistance = float.PositiveInfinity;
        float distance = Vector3.Distance(transform.position, targetPos);

        yield return null;
        anim.Play("Jump");
        while (prevDistance >= distance)
        {
            prevDistance = distance;
            distance = Vector3.Distance(transform.position, targetPos);

            yield return null;
        }
    }
    public IEnumerator ChasePlayer(float time)
    {
        bool startedGrounded = isGrounded;
        bool left = GameManager.playerPosition.x < transform.position.x;
        float cooldown = 0.0f;

        LookAtPlayer();

        anim.Play(isGrounded ? "Run" : "Jump");

        while (time > 0.0f)
        {
            if (cooldown <= 0.0f && left != (GameManager.playerPosition.x < transform.position.x))
            {
                left = GameManager.playerPosition.x < transform.position.x;
                cooldown = 0.4f;
                time += 0.1f;
            }

            if (cooldown <= 0.0f)
                body.velocity = new Vector2(transform.right.x * anim.transform.lossyScale.x * -120f, body.velocity.y);
            else
            {
                cooldown -= Time.deltaTime;
                if (cooldown <= 0.0f)
                    LookAtPlayer();
            }

            if (startedGrounded != isGrounded)
                anim.Play("Run");

            time -= Time.deltaTime;
            yield return null;
        }

        body.velocity = new Vector2(0, body.velocity.y);
    }
    public IEnumerator Dash()
    {
        body.gravityScale = 0.0f;

        anim.Play("Dash");
        int right = (GameManager.playerPosition.x > transform.position.x ? 1 : -1);

        LookAtPlayer();
        body.velocity = transform.right * -200f * transform.localScale.x;

        while ((GameManager.playerPosition.x - transform.position.x) * right > 0 && !isWalled)
            yield return null;

        anim.Play("Jump");
        body.velocity = Vector3.zero;

        body.gravityScale = 100.0f;
    }
    public IEnumerator Earthquake()
    {
        anim.Play("Jump");
        while (!isGrounded)
            yield return null;

        anim.Play("Punch");
        yield return new WaitForSeconds(0.2f);

        GameManager.ShakeCamera(0.5f, 2f, true);

        yield return new WaitForSeconds(0.25f);

        yield return null;
    }

    public IEnumerator FixedThunder()
    {
        body.velocity = Vector2.zero;
        body.isKinematic = true;

        anim.Play("Thunder");

        bool startRight = (GameManager.playerPosition - rightCorner.position).sqrMagnitude < (GameManager.playerPosition - leftCorner.position).sqrMagnitude;
        Vector3 startPos = (rightCorner.position + leftCorner.position) * 0.5f + Vector3.up * 100;

        yield return new WaitForSeconds(0.75f);

        for (int i =  0; i < 6; i++)
        {
            Vector3 targetPos = startPos + Vector3.right * 16 * i;
            GameObject thunder = Instantiate(thunderObj);
            thunder.transform.position = targetPos;

            targetPos = startPos + Vector3.left * 16 * i;
            thunder = Instantiate(thunderObj);
            thunder.transform.position = targetPos;
            yield return new WaitForSeconds(0.08f);
        }

        anim.Play("Thunder", -1, 0f);
        yield return new WaitForSeconds(0.5f);

        startRight = (GameManager.playerPosition - rightCorner.position).sqrMagnitude < (GameManager.playerPosition - leftCorner.position).sqrMagnitude;
        startPos = (startRight ? rightCorner.position : leftCorner.position) + Vector3.up * 100;
        for (int i = 0; i < 15; i++)
        {
            Vector3 targetPos = startPos + (startRight ? Vector3.left : Vector3.right) * 16 * i;

            GameObject thunder = Instantiate(thunderObj);
            thunder.transform.position = targetPos;
            yield return new WaitForSeconds(0.082f);
        }

        body.isKinematic = false;
        anim.Play("Jump");

        while (!isGrounded)
            yield return null;
    }
    public IEnumerator JetSliders(int loops)
    {
        while (!isGrounded)
            yield return null;

        anim.Play("FingerGun");

        bool startRight = (GameManager.playerPosition - rightCorner.position).sqrMagnitude < (GameManager.playerPosition - leftCorner.position).sqrMagnitude;
        transform.eulerAngles = Vector3.up * (startRight ? 0f : 180f);
        yield return new WaitForSeconds(0.4f);


        for (int j = 0; j < loops; j++)
        {
            transform.eulerAngles = Vector3.up * (startRight ? 0f : 180f);
            for (int i = 0; i < 6; i++)
            {
                GameObject slider = Instantiate(sliderObj);
                slider.transform.position = (startRight ? rightCorner.position : leftCorner.position) + Vector3.up * (8 + i * 14) + (startRight ? Vector3.right : Vector3.left) * 14 * i;
                slider.transform.localScale = new Vector3(1, startRight ? -1 : 1, 1);
                slider.transform.rotation = Quaternion.Euler(0, 0, startRight ? 180 : 0);
                slider.GetComponent<EnWp_Shot>().direction = startRight ? Vector2.left : Vector2.right;
            }

            startRight = !startRight;
            yield return new WaitForSeconds(1f);
        }

        anim.Play("Stand");
        yield return new WaitForSeconds(0.25f);
    }
    public IEnumerator SlidersHor(int loops)
    {
        while (!isGrounded)
            yield return null;

        anim.Play("FingerGun");

        bool startRight = (GameManager.playerPosition - rightCorner.position).sqrMagnitude < (GameManager.playerPosition - leftCorner.position).sqrMagnitude;
        transform.eulerAngles = Vector3.up * (startRight ? 0f : 180f);
        yield return new WaitForSeconds(0.4f);


        for (int j = 0; j < loops; j++)
        {
            transform.eulerAngles = Vector3.up * (startRight ? 0f : 180f);
            for (int i = 0; i < 6; i++)
            {
                GameObject slider = Instantiate(sliderObj);
                slider.transform.position = (startRight ? rightCorner.position : leftCorner.position) + Vector3.up * (8 + i * 40);
                slider.transform.localScale = new Vector3(1, startRight ? -1 : 1, 1);
                slider.transform.rotation = Quaternion.Euler(0, 0, startRight ? 180 : 0);
                slider.GetComponent<EnWp_Shot>().direction = startRight ? Vector2.left : Vector2.right;

                yield return new WaitForSeconds(0.25f);
            }

            startRight = !startRight;
            yield return new WaitForSeconds(0.1f);
        }

        anim.Play("Stand");
        yield return new WaitForSeconds(0.25f);
    }
    public IEnumerator SlidersVer(int loops)
    {
        while (!isGrounded)
            yield return null;

        anim.Play("FingerGun");

        bool startUp = true;
        bool startRight = (GameManager.playerPosition - rightCorner.position).sqrMagnitude < (GameManager.playerPosition - leftCorner.position).sqrMagnitude;
        LookAtPlayer();
        yield return new WaitForSeconds(0.4f);


        for (int j = 0; j < loops; j++)
        {
            LookAtPlayer();
            for (int i = 0; i < 6; i++)
            {
                GameObject slider = Instantiate(sliderObj);
                slider.transform.position = (startUp ? rightCorner.position : leftCorner.position) + (startUp ? Vector3.up * 240 : Vector3.down * 32);
                slider.transform.position = new Vector3(startRight ? (rightCorner.position.x - 48 * i) : (leftCorner.position.x + 40 * i) + (startUp ? 0 : 20),
                                                        slider.transform.position.y, slider.transform.position.z);
                slider.transform.localScale = new Vector3(-1, (startRight ^ startUp) ? -1 : 1, 1);
                slider.transform.rotation = Quaternion.Euler(0, 0, startUp ? 90 : 270);
                slider.GetComponent<EnWp_Shot>().direction = startUp ? Vector2.down : Vector2.up;

                yield return new WaitForSeconds(0.1f);
            }

            startUp = !startUp;
            yield return new WaitForSeconds(1f);
        }

        anim.Play("Stand");
        yield return new WaitForSeconds(0.25f);
    }
    public IEnumerator HeartShot()
    {
        LookAtPlayer();
        while (!isGrounded)
            yield return null;

        anim.Play("BigBuster");
        yield return new WaitForSeconds(1.0f);

        GameObject heart = Instantiate(heartShotObj);
        heart.transform.position = transform.position + transform.right * transform.localScale.x * -16f;
        heart.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, GameManager.playerPosition - heart.transform.position));

        yield return new WaitForSeconds(0.2f);
    }
    public IEnumerator Shoot_MegaBuster(int shots)
    {
        if (isGrounded)
            anim.Play("Shoot");
        else
            anim.Play("ShootAir");

        bool startedGrounded = isGrounded;

        for (int i = 0; i < shots; i++)
        {
            GameObject shot;
            if (i == shots - 1)
                shot = Shoot(transform.position - transform.right * anim.transform.lossyScale.x * 30f, -transform.right * anim.transform.lossyScale.x, 3, 300, charge1Obj);
            else if (i == shots - 2)
                shot = Shoot(transform.position - transform.right * anim.transform.lossyScale.x * 30f, -transform.right * anim.transform.lossyScale.x, 3, 300, charge2Obj);
            else
                shot = Shoot(transform.position - transform.right * anim.transform.lossyScale.x * 30f, -transform.right * anim.transform.lossyScale.x, 3, 300);

            float time = isGrounded ? 0.2f : 0.1f;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;

                if (!startedGrounded && isGrounded)
                {
                    startedGrounded = isGrounded;
                    anim.Play("Shoot");
                }
                yield return null;
            }
        }

    }
    public IEnumerator Rollnado(int loops)
    {
        yield return Jump(false, 0);

        anim.Play("Rollnado");
        body.gravityScale = 200f;
        shielded = true;

        for (int i = 0; i < loops; i++)
        {
            while (!isGrounded)
                yield return null;
            yield return null;

            body.velocity = Vector3.up * 600f + (GameManager.playerPosition.x > transform.position.x ? Vector3.right : Vector3.left) * Mathf.Min(100f, Mathf.Abs(GameManager.playerPosition.x - transform.position.x) + 50);

            while (body.velocity.y > 0.0f)
                yield return null;
        }

        body.velocity = Vector3.zero;

        shielded = false;
        body.gravityScale = 100f;
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
                Physics2D.OverlapBox((Vector2)transform.position + col.offset - (Vector2)transform.right * transform.localScale.x, new Vector2(col.size.x, col.size.y * 0.9f), 0, 1 << 8);
        }
    }


}

