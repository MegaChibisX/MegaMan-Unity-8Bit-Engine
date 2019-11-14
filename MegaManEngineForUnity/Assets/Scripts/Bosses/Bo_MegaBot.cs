using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_MegaBot : Boss
{

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;

    public AudioClip healSound;

    public SpriteRenderer spr_dark;
    public SpriteRenderer spr_light;
    public SpriteRenderer spr_outline;
    public SpriteRenderer spr_solids;

    public Misc_CopySprite cοpy_dark;
    public Misc_CopySprite cοpy_light;
    public Misc_CopySprite cοpy_outline;
    public Misc_CopySprite cοpy_solids;

    private GameManager.Players curPlayer;
    public Pl_WeaponData.Weapons curWeapon;
    public Pl_WeaponData.WeaponColors defaultColors;
    public Pl_WeaponData.WeaponColors[] chargeColors;

    private float charge = 0.0f;

    [Header("--------")]
    public GameObject spawnParticles;
    public GameObject chargedShot;
    public GameObject chargedShotProto;
    public GameObject hyperBomb;
    public GameObject metalBlade;
    public GameObject geminiLaser;
    public GameObject pharaohShot;
    public GameObject starMeteor;
    public GameObject windWheel;
    public GameObject windObject;
    public GameObject blackHole;
    public GameObject commandoBomb;

    protected override void Start()
    {
        // Destroyes the boss if the boss should be dead.
        //if (GameManager.bossDead_ && !ignorePreviousDeath)
        //    Destroy(gameObject);

        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponentInChildren<SpriteRenderer>();
        aud = GetComponent<AudioSource>();
        rend.enabled = false;

        anim.gameObject.SetActive(false);
    }
    protected override void LateUpdate()
    {
        if (invisTime % 0.2f > 0.07f)
            rend.sprite = hurtSprite;
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;

        if (charge > 0.0f && chargeColors.Length > 0)
        {
            int index = Mathf.FloorToInt((Time.unscaledTime % 0.2f) * chargeColors.Length * 5);
            index = Mathf.Clamp(index, 0, chargeColors.Length - 1);
            ChangeColors(chargeColors[index]);
        }
    }

    public override void Damage(Pl_Weapon weapon)
    {
        float damage = weapon.damage;

        if (curWeapon == Pl_WeaponData.Weapons.BlackHoleBomb)
            damage *= 1.5f;
        else
            damage = 1;

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
        //if (!ignorePreviousDeath)
        //    GameManager.bossDead_ = true;
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.aud.Stop();
        if (GameManager.maxFortressStage == 2)
        {
            GameManager.maxFortressStage = 3;
            GameManager.playFortressStageUnlockAnimation = true;
        }
    }
    public override void LookAtPlayer()
    {
        anim.transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? 1 : -1, 1, 1);
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
        anim.Play("Stand");

        // Sets the player colors.
        spr_dark.enabled = false;
        spr_light.enabled = false;
        spr_outline.enabled = false;
        yield return CopyWeapon();

        yield return new WaitForSeconds(1.0f);

        spr_dark.enabled = true;
        SpawnParticles(transform.position);
        GameManager.ShakeCamera(0.3f, 1f, false);
        yield return new WaitForSeconds(0.75f);
        spr_light.enabled = true;
        SpawnParticles(transform.position);
        GameManager.ShakeCamera(0.3f, 1f, false);
        yield return new WaitForSeconds(0.75f);
        spr_outline.enabled = true;


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

            //yield return CopyWeapon();
            yield return SetWeapon((Pl_WeaponData.Weapons)Random.Range(0, (int)Pl_WeaponData.Weapons.Length));
            yield return JumpAtPlayer_Parent();
            yield return SlideAtPlayer();


            switch (curWeapon)
            {
                default:
                case Pl_WeaponData.Weapons.MegaBuster:
                    yield return Jump();
                    while (GameManager.playerPosition.y < transform.position.y && !isGrounded)
                        yield return null;

                    if (Random.Range(0, 3) != 0)
                    {
                        yield return Shoot_MegaBuster(3);
                        while (!isGrounded)
                            yield return null;
                    }
                    else
                    {
                        StartCoroutine(Charge(1f));
                        yield return ChasePlayer(1.5f);
                        body.velocity = Vector2.zero;
                        anim.Play("Stand");
                        yield return new WaitForSeconds(0.5f);
                        yield return Shoot_MegaBuster(1);
                    }
                    break;
                case Pl_WeaponData.Weapons.BassBuster:
                    yield return Jump();

                    if (Random.Range(0, 3) != 0)
                    {
                        if (charge > 0.0f)
                            yield return Shoot_MegaBuster(1);
                        for (int i = 0; i < 2; i++)
                        {
                            yield return Shoot_Bass(4 + i * 2);

                            anim.Play(isGrounded ? "Stand" : "Jump");
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        StartCoroutine(Charge(1f));
                        yield return ChasePlayer(1.5f);
                    }
                    break;
                case Pl_WeaponData.Weapons.HyperBomb:
                    yield return new WaitForSeconds(0.25f);
                    yield return JumpAtPlayer_Parent();
                    yield return new WaitForSeconds(0.125f);
                    yield return Shoot_Bomb();
                    break;
                case Pl_WeaponData.Weapons.MetalBlade:
                    yield return Shoot_Metal();
                    break;
                case Pl_WeaponData.Weapons.GeminiLaser:
                    yield return Shoot_Gemini();
                    break;
                case Pl_WeaponData.Weapons.PharaohShot:
                    for (int i = 0; i < 3; i++)
                        yield return Shoot_Pharaoh();
                    break;
                case Pl_WeaponData.Weapons.StarCrash:
                    yield return Shoot_Star();
                    break;
                case Pl_WeaponData.Weapons.WindStorm:
                    yield return Shoot_Wind();
                    break;
                case Pl_WeaponData.Weapons.BlackHoleBomb:
                    yield return Shoot_BlackHole();
                    break;
                case Pl_WeaponData.Weapons.CommandoBomb:
                    yield return Shoot_Commando();
                    break;
            }

            anim.Play("Stand");

            yield return new WaitForSeconds(0.25f);
        }
    }

    public IEnumerator CopyWeapon()
    {
        yield return new WaitForSeconds(0.125f);

        yield return SetWeapon((Pl_WeaponData.Weapons)Player.instance.currentWeaponIndex);
    }
    public IEnumerator SetWeapon(Pl_WeaponData.Weapons weapon)
    {
        Pl_WeaponData.Weapons oldWeapon = curWeapon;
        Pl_WeaponData wpn = Pl_WeaponData.WeaponList[(int)weapon];

        if (Player.instance != null)
        {
            if (Player.instance.bodyColorDark && Player.instance.bodyColorDark.GetComponent<Misc_CopySprite>() != null)
                cοpy_dark.spritePath = Player.instance.bodyColorDark.GetComponent<Misc_CopySprite>().spritePath;
            if (Player.instance.bodyColorLight && Player.instance.bodyColorLight.GetComponent<Misc_CopySprite>() != null)
                cοpy_light.spritePath = Player.instance.bodyColorLight.GetComponent<Misc_CopySprite>().spritePath;
            if (Player.instance.bodyColorOutline && Player.instance.bodyColorOutline.GetComponent<Misc_CopySprite>() != null)
                cοpy_outline.spritePath = Player.instance.bodyColorOutline.GetComponent<Misc_CopySprite>().spritePath;
            cοpy_solids.spritePath = GameManager.GetPlayerSpritePath(Player.instance.curPlayer);
            cοpy_solids.spritePath = cοpy_solids.spritePath.Substring(0, cοpy_solids.spritePath.Length - 4) + "Blank";

            if (wpn != null)
            {
                defaultColors = wpn.baseColors;

                if (defaultColors.colorDark.a == 0)
                    spr_dark.color = Player.instance.defaultColors.colorDark;
                else
                    spr_dark.color = defaultColors.colorDark;
                if (defaultColors.colorLight.a == 0)
                    spr_light.color = Player.instance.defaultColors.colorLight;
                else
                    spr_light.color = defaultColors.colorLight;
                if (defaultColors.colorOutline.a == 0)
                {
                    spr_outline.color = Player.instance.defaultColors.colorOutline;
                    spr_solids.color = Player.instance.defaultColors.colorOutline;
                }
                else
                {
                    spr_outline.color = defaultColors.colorOutline;
                    spr_solids.color = defaultColors.colorOutline;
                }

                ChangeColors(new Pl_WeaponData.WeaponColors(spr_light.color, spr_dark.color, spr_outline.color));
                defaultColors = new Pl_WeaponData.WeaponColors(spr_light.color, spr_dark.color, spr_outline.color);
            }
            else
            {
                spr_dark.color = Player.instance.defaultColors.colorDark;
                spr_light.color = Player.instance.defaultColors.colorLight;
                spr_outline.color = Player.instance.defaultColors.colorOutline;
                spr_solids.color = Player.instance.defaultColors.colorOutline;

                defaultColors = new Pl_WeaponData.WeaponColors(spr_light.color, spr_dark.color, spr_outline.color);
            }

            cοpy_dark.Reload();
            cοpy_light.Reload();
            cοpy_outline.Reload();
            cοpy_solids.Reload();

            curPlayer = Player.instance.curPlayer;
            curWeapon = weapon;
        }
        else
        {
            cοpy_solids.spritePath = GameManager.GetPlayerSpritePath(GameManager.Players.MegaMan);
            cοpy_solids.spritePath = cοpy_solids.spritePath.Substring(0, cοpy_solids.spritePath.Length - 4) + "Blank";
            cοpy_solids.Reload();

            curWeapon = weapon;
        }

        if (curWeapon != oldWeapon)
            charge = 0.0f;

        yield return new WaitForSeconds(0.125f);
    }
    public void ChangeColors(Pl_WeaponData.WeaponColors colors)
    {
        spr_dark.color = colors.colorDark;
        if (spr_dark.color.a == 0)
            spr_dark.color = defaultColors.colorDark;
        spr_light.color = colors.colorLight;
        if (spr_light.color.a == 0)
            spr_light.color = defaultColors.colorLight;
        spr_outline.color = colors.colorOutline;
        spr_solids.color = colors.colorOutline;
        if (spr_outline.color.a == 0)
        {
            spr_outline.color = defaultColors.colorOutline;
            spr_solids.color = defaultColors.colorOutline;
        }
    }

    public IEnumerator Jump()
    {
        anim.Play("Jump");

        LookAtPlayer();
        body.velocity = new Vector2(body.velocity.x, 370f);

        while (body.velocity.y > 0)
            yield return null;
    }
    public IEnumerator JumpAtPlayer_Parent()
    {
        switch (curPlayer)
        {
            case GameManager.Players.Bass:
                yield return JumpAtPlayer_Bass();
                break;
            default:
                yield return JumpAtPlayer();
                break;
        }
    }
    public IEnumerator JumpAtPlayer()
    {
        Vector3 targetPos = GameManager.playerPosition;
        Vector3 force = Helper.LaunchVelocity(transform.position, targetPos, 75f, 1000f);

        anim.transform.localScale = new Vector3(targetPos.x > transform.position.x ? 1 : -1, 1, 1);

        anim.Play("Jump");
        body.velocity = force;
        while (!isGrounded)
            yield return null;

        body.velocity = Vector3.zero;
        anim.Play("Stand");
    }
    public IEnumerator JumpAtPlayer_Bass()
    {
        Vector3 targetPos = GameManager.playerPosition;
        Vector3 force = Helper.LaunchVelocity(transform.position, transform.position * 0.5f + targetPos * 0.5f, 75f, 1000f);

        anim.transform.localScale = new Vector3(targetPos.x > transform.position.x ? 1 : -1, 1, 1);

        anim.Play("Jump");
        body.velocity = force;
        while (body.velocity.y > 0)
            yield return null;

        targetPos = GameManager.playerPosition;
        force = Helper.LaunchVelocity(transform.position, targetPos, 70f, 1000f);

        anim.transform.localScale = new Vector3(targetPos.x > transform.position.x ? 1 : -1, 1, 1);

        body.velocity = force;
        while (!isGrounded)
            yield return null;

        body.velocity = Vector3.zero;
        anim.Play("Stand");
    }

    public IEnumerator SlideAtPlayer()
    {
        float slideSpeed = 160f;
        float distance = Mathf.Abs(GameManager.playerPosition.x - transform.position.x);
        float time = distance / slideSpeed + 0.25f;

        anim.transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? 1 : -1, 1, 1);

        anim.Play("Slide");
        while (time > 0.0f)
        {
            body.velocity = transform.right * anim.transform.localScale.x * slideSpeed;
            time -= Time.deltaTime;
            yield return null;
        }

        anim.Play("Stand");
        body.velocity = Vector3.zero;
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
                cooldown = 0.2f;
                time += 0.1f;
            }

            if (cooldown <= 0.0f)
                body.velocity = new Vector2(anim.transform.lossyScale.x * 120f, body.velocity.y);
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

    public IEnumerator Charge(float time)
    {
        chargeColors = new Pl_WeaponData.WeaponColors[0];
        while (time > 0.0f)
        {
            time -= Time.deltaTime;
            charge += Time.deltaTime;

            if (charge > 1.0f)
            {
                Pl_WeaponData wpn = Pl_WeaponData.WeaponList[(int)curWeapon];
                if (wpn.chargeColors != null && wpn.chargeColors.Length > 0)
                {
                    int index = Mathf.Min(1, wpn.chargeColors.Length - 1);
                    chargeColors = new Pl_WeaponData.WeaponColors[wpn.chargeColors.Length / 2];
                    for (int i = 0; i < chargeColors.Length; i++) {
                        chargeColors[i] = wpn.chargeColors[index, i];
                    }
                }
            }
            else if (charge > 0.3f)
            {
                Pl_WeaponData wpn = Pl_WeaponData.WeaponList[(int)curWeapon];
                if (wpn.chargeColors != null && wpn.chargeColors.Length > 0)
                {
                    int index = Mathf.Min(0, wpn.chargeColors.Length - 1);
                    chargeColors = new Pl_WeaponData.WeaponColors[wpn.chargeColors.Length / 2];
                    for (int i = 0; i < chargeColors.Length; i++)
                    {
                        chargeColors[i] = wpn.chargeColors[index, i];
                    }
                }
            }

            yield return null;
        }
    }
    public IEnumerator Shoot_MegaBuster(int shots)
    {
        if (isGrounded)
            anim.Play("StandShoot");
        else
            anim.Play("JumpShoot");

        bool startedGrounded = isGrounded;

        for  (int i = 0; i < shots; i++)
        {
            GameObject shot;
            if (charge >= 1.0f)
            {
                if (curPlayer == GameManager.Players.ProtoMan)
                    shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 5, 300, chargedShotProto);
                else
                    shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 5, 300, chargedShot);
            }
            else
                shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 3, 300);

            shot.transform.localScale = anim.transform.localScale;
            

            charge = 0.0f;
            ChangeColors(defaultColors);
            float time = isGrounded ? 0.2f : 0.1f;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;

                if (!startedGrounded && isGrounded)
                {
                    startedGrounded = isGrounded;
                    anim.Play("StandShoot");
                }
                yield return null;
            }
        }
    }
    public IEnumerator Shoot_Bass(int shots)
    {
        if (isGrounded)
            anim.Play("StandShoot");
        else
            anim.Play("JumpShoot");

        bool startedGrounded = isGrounded;

        for (int i = 0; i < shots; i++)
        {
            GameObject shot;
            if (charge >= 1.0f)
            {
                if (curPlayer == GameManager.Players.ProtoMan)
                    shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 5, 500, chargedShotProto);
                else
                    shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 5, 500, chargedShot);
            }
            else
                shot = Shoot(transform.position + transform.right * anim.transform.lossyScale.x * 30f, transform.right * anim.transform.lossyScale.x, 3, 300);

            shot.transform.localScale = anim.transform.localScale;


            charge = 0.0f;
            ChangeColors(defaultColors);
            float time = 0.05f;
            while (time > 0.0f)
            {
                time -= Time.deltaTime;

                if (!startedGrounded && isGrounded)
                {
                    startedGrounded = isGrounded;
                    anim.Play("StandShoot");
                }
                yield return null;
            }
        }
    }
    public IEnumerator Shoot_Bomb()
    {
        while (!isGrounded)
            yield return null;

        GameObject bomb = Instantiate(hyperBomb);
        bomb.transform.position = transform.position + transform.up * 12f;

        Vector3 velocity = Helper.LaunchVelocity(bomb.transform.position, GameManager.playerPosition, 60, 1000);
        bomb.GetComponent<Rigidbody2D>().velocity = velocity;

        anim.Play("StandThrow");
        LookAtPlayer();

        yield return new WaitForSeconds(0.1f);

        anim.Play("Stand");
    }
    public IEnumerator Shoot_Metal()
    {
        float jumpHeight = (400f + Mathf.Min(200f, Mathf.Abs(transform.position.y - GameManager.playerPosition.y) * 3));
        body.velocity = transform.up * jumpHeight;

        anim.Play("Jump");

        while (body.velocity.y > 0)
            yield return null;

        anim.Play("JumpThrow");
        LookAtPlayer();


        int jumps = (int)((jumpHeight - 300) / 75) + 1;
        for (int i = 0; i < jumps; i++)
        {
            Shoot(transform.position, GameManager.playerPosition - transform.position, 1f, 300, metalBlade);
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
    public IEnumerator Shoot_Gemini()
    {
        yield return null;

        LookAtPlayer();

        while (!isGrounded)
            yield return null;

        body.velocity = Vector2.zero;
        anim.Play("StandShoot");
        LookAtPlayer();

        yield return new WaitForSeconds(0.25f);


        for (int i = 0; i < 3; i++)
        {
            GameObject shot = Instantiate(geminiLaser);
            shot.transform.position = transform.position + transform.right * anim.transform.localScale.x * (16f + i * 6);
            shot.GetComponent<SpriteRenderer>().flipX = transform.right.x * anim.transform.localScale.x > 0;

            shot.GetComponent<EnWp_GeminiLaser>().shotOrder = i;
            if (i == 0)
                Helper.PlaySound(shot.GetComponent<EnWp_GeminiLaser>().geminiNoise);
        }

        yield return new WaitForSeconds(0.15f);

        yield return null;
        body.isKinematic = false;
    }
    public IEnumerator Shoot_Pharaoh()
    {
        Vector3 targetPos = GameManager.playerPosition;
        Vector3 force = Helper.LaunchVelocity(transform.position, targetPos, 75f, 1000f);

        anim.transform.localScale = new Vector3(targetPos.x > transform.position.x ? 1 : -1, 1, 1);

        anim.Play("Jump");
        body.velocity = force;
        while (body.velocity.y > 0.0f)
            yield return null;
        LookAtPlayer();

        anim.Play("JumpThrow");

        GameObject shot = Shoot(transform.position, GameManager.playerPosition - transform.position, 3, 400, pharaohShot);
        shot.GetComponent<Rigidbody2D>().velocity = (GameManager.playerPosition - transform.position).normalized * 400.0f;

        while (!isGrounded)
            yield return null;

        body.velocity = Vector2.zero;
        anim.Play("Stand");
    }
    public IEnumerator Shoot_Star()
    {
        while (!isGrounded)
            yield return null;

        body.velocity = Vector2.zero;
        anim.Play("Stand");

        yield return new WaitForSeconds(0.2f);
        anim.Play("StandThrow");

        for (int i = 0; i < 5; i++)
        {
            GameObject star = Instantiate(starMeteor);
            star.transform.position = new Vector3(GameManager.playerPosition.x, transform.position.y) + Vector3.up * 150;
            GameManager.ShakeCamera(0.2f, 3f, false);
            LookAtPlayer();
            SpawnParticles(star.transform.position);

            yield return new WaitForSeconds(0.5f);
        }
    }
    public IEnumerator Shoot_Wind()
    {
        anim.Play("Shoot");
        LookAtPlayer();

        GameObject windArea = Instantiate(windObject);
        windArea.transform.position = transform.position;

        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.5f);

            EnWp_WaveShot newShot = Instantiate(windWheel).GetComponent<EnWp_WaveShot>();
            newShot.transform.position = transform.position;
            newShot.transform.localScale = new Vector3(anim.transform.localScale.x * transform.right.x, transform.localScale.y, transform.localScale.z);

            newShot = Instantiate(windWheel).GetComponent<EnWp_WaveShot>();
            newShot.transform.position = transform.position;
            newShot.transform.localScale = new Vector3(anim.transform.localScale.x * transform.right.x, transform.localScale.y, transform.localScale.z);
            newShot.waveLength *= -1f;
        }
    }
    public IEnumerator Shoot_BlackHole()
    {
        while (!isGrounded)
            yield return null;
        body.velocity = Vector2.zero;
        anim.Play("StandShoot");
        LookAtPlayer();

        Enemy hole = Instantiate(blackHole.gameObject).GetComponent<Enemy>();
        hole.transform.position = transform.position;
        hole.GetComponent<Rigidbody2D>().gravityScale = -10.0f;

        yield return new WaitForSeconds(0.3f);

        Vector3 holePos = hole.transform.position;
        SpawnParticles(holePos);
        hole.Kill(false, false);

        float time = (hole.transform.position.x - transform.position.x) / 120f;
        yield return ChasePlayer(time);

        anim.Play("StandShoot");
        time = 4.5f - time;
        float cooldown = 0.5f;
        while (time > 0.0f)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0.0f)
            {
                LookAtPlayer();
                Shoot(transform.position, GameManager.playerPosition - transform.position, 1, 300);
                cooldown = 0.5f;
            }

            time -= Time.deltaTime;
            yield return null;
        }

        SpawnParticles(holePos);
    }
    public IEnumerator Shoot_Commando()
    {
        if (isGrounded)
            anim.Play("StandShoot");
        else
            anim.Play("JumpShoot");
        LookAtPlayer();

        yield return new WaitForSeconds(0.35f);

        EnWp_CommandoBomb newBomb = Instantiate(commandoBomb.gameObject).GetComponent<EnWp_CommandoBomb>();
        newBomb.transform.position = transform.position + transform.right * anim.transform.localScale.x * 32;
        newBomb.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        newBomb.startDir = transform.right * anim.transform.localScale.x;

        yield return new WaitForSeconds(0.15f);
        body.bodyType = RigidbodyType2D.Dynamic;
    }

    public void SpawnParticles(Vector3 pos)
    {
        GameObject obj = Instantiate(spawnParticles);
        obj.transform.position = pos;
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
