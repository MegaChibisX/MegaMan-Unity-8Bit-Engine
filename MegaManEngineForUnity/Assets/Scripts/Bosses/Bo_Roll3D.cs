using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_Roll3D : Boss
{

    private Animator anim;
    private AudioSource aud;
    public AudioClip newTrack;
    public AudioClip victoryAudioClip;

    public AudioClip healSound;

    public SkinnedMeshRenderer[] rends;
    public Material[]            mats;
    public Material flashMat;

    public Collider2D handR_Tri;
    public Collider2D handL_Tri;
    public Collider2D handR_Col;
    public Collider2D handL_Col;
    public Collider2D handR_Tal;
    public Collider2D handL_Tal;

    public Transform mouthBone;

    public Texture2D head2Norm;
    public Texture2D head2Emis;
    public Texture2D body2Norm;
    public Texture2D body2Emis;

    public GameObject chargedShot;
    public GameObject charged2Shot;
    public GameObject rockDrop;

    private float headRotation = 0.0f;
    private bool headMoving = false;
    private bool lowerHeight = false;
    // This is assigned from the Player script.
    [System.NonSerialized]
    public bool hitPlayer = false;

    private int phase = 0;
    private bool inPhase1_5;
    private bool inPhase2;
    private float phase2Height = 1f;

    protected override void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        handR_Col.gameObject.SetActive(false);
        handL_Col.gameObject.SetActive(false);
        handR_Tri.gameObject.SetActive(false);
        handL_Tri.gameObject.SetActive(false);
        handR_Tal.gameObject.SetActive(false);
        handL_Tal.gameObject.SetActive(false);

        mats[0] = new Material(rends[0].material);
        mats[1] = new Material(rends[1].material);

        rends[0].material = mats[0];
        rends[1].material = mats[1];

        anim.gameObject.SetActive(false);
    }
    protected void Update()
    {
        RotateHead();

        if (inPhase1_5)
        {
            if (lowerHeight)
                phase2Height = Mathf.MoveTowards(phase2Height, 0f, 2f * Time.deltaTime);
            else
                phase2Height = Mathf.Clamp01(phase2Height - Time.deltaTime * 0.1f);

            anim.SetLayerWeight(3, phase2Height);
        }
        else
            anim.SetLayerWeight(3, 0);

        float color = Mathf.Abs((Time.time * 4f) % 2 - 1);
        color = Mathf.Clamp01(health / 56 + (color <= 1 ? color : 1f - color) * 0.2f);
        if (phase == 0)
            color = 1;
        mats[0].SetColor("_ColorEmis", new Color(color, color, color, 1));
        mats[1].SetColor("_ColorEmis", new Color(color, color, color, 1));
        flashMat.SetColor("_ColorEmis", new Color(color, color, color, 1));
    }
    protected override void LateUpdate()
    {
        if (rends[0] == null)
            return;

        if (invisTime % 0.2f > 0.07f)
        {
            //for (int i = 0; i < rends.Length; i++)
                rends[0].sharedMaterial = flashMat;
        }
        else
        {
            //for (int i = 0; i < rends.Length; i++)
                rends[0].sharedMaterial = mats[0];
        }
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }
    protected override void OnGUI()
    {
        // Draws the health bar if the fight has started.
        if (fightStarted)
        {
            float x = Camera.main.pixelWidth / 256.0f;
            float y = Camera.main.pixelHeight / 218.0f;
            Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);

            Sprite healthBar = healthBarFull;
            Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                    healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
            Sprite emptyBar = healthBarEmpty;
            Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                    emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 28; i++)
                {
                    if (health > i + j * 28)
                        GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 40f + x * j * 8, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
                    else
                        GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 40f + x * j * 8, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
                }
            }
        }
    }

    public override void Damage(Pl_Weapon weapon)
    {
        float damage = weapon.damage;

        if (weapon.weaponType == Pl_Weapon.WeaponTypes.Normal)
            damage = Mathf.Min(damage, 1);
        else if (weapon.weaponType == Pl_Weapon.WeaponTypes.Gemini)
            damage = 0.2f;
        else if (weapon.weaponType == Pl_Weapon.WeaponTypes.Galaxy)
            damage = 0.1f;
        else
            damage = 0;


        if ((shielded && !weapon.ignoreShield) || damage == 0)
        {
            weapon.Deflect();
            return;
        }

        if (!lowerHeight)
            phase2Height = Mathf.Clamp01(phase2Height + (phase == 1 ? 0.3f : 0.25f) * damage);

        Damage(damage, weapon.ignoreInvis);
    }
    public override void Damage(float dmg, bool ignoreInvis)
    {
        base.Damage(dmg, ignoreInvis);

        if (health <= 28.0f && phase == 1)
        {
            StopAllCoroutines();
            StartCoroutine(Behavior2());
        }
    }
    public override void Kill(bool makeItem, bool makeBolt)
    {
        StopAllCoroutines();

        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.PlayMusic(null);
    }
    public void RotateHead()
    {
        if (headMoving)
        {
            float distance = Mathf.Sin(Vector2.SignedAngle(Vector2.down, GameManager.playerPosition - mouthBone.parent.parent.position) * Mathf.Deg2Rad);
            headRotation = distance * Mathf.Abs(distance);
        }
        headRotation = Mathf.Clamp(headRotation, -1f, 1f);
        anim.SetLayerWeight(1, Mathf.Clamp01(-headRotation));
        anim.SetLayerWeight(2, Mathf.Clamp01( headRotation));
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


        Color backColor = CameraCtrl.instance.GetComponent<Camera>().backgroundColor;
        for (int i = 0; i <= 5; i++)
        {
            CameraCtrl.instance.GetComponent<Camera>().backgroundColor = backColor * (5 - i) * 0.2f + (Color)(new Color32(12, 12, 14, 255)) * i * 0.2f;
            GameManager.ShakeCamera(0.2f, 2f, false);

            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1.0f);

        // Plays intro.
        anim.gameObject.SetActive(true);
        anim.Play("Intro");
        GameManager.ShakeCamera(13, 3, false);
        yield return new WaitForSeconds(14.0f);
        anim.Play("Laugh");
        CameraCtrl.instance.aud.PlaySound(newTrack, true);

        // Slowly gains health.
        for (int i = 0; i < 56; i++)
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
        headMoving = true;
        StartCoroutine(Behavior());
    }
    private IEnumerator StartPhase2()
    {
        handL_Col.gameObject.SetActive(false);
        handL_Tri.gameObject.SetActive(false);
        handL_Tal.gameObject.SetActive(false);
        handR_Col.gameObject.SetActive(false);
        handR_Tri.gameObject.SetActive(false);
        handR_Tal.gameObject.SetActive(false);

        GameManager.ShakeCamera(0.2f, 3f, false);

        phase = 2;
        inPhase2 = true;
        headMoving = false;
        headRotation = 0.0f;
        canBeHit = false;
        phase2Height = 0.0f;

        anim.Play("Phase2Start");
        yield return new WaitForSeconds(2.0f);

        phase2Height = 0.0f;

        GameManager.ShakeCamera(0.3f, 3f, false);
        mats[0].SetTexture("_MainTex", head2Norm);
        mats[0].SetTexture("_EmisTex", head2Emis);
        mats[1].SetTexture("_MainTex", body2Norm);
        mats[1].SetTexture("_EmisTex", body2Emis);

        yield return new WaitForSeconds(4.5f);

        canBeHit = true;
        headMoving = true;
    }
    private IEnumerator StartPhase1_5()
    {
        handL_Col.gameObject.SetActive(false);
        handL_Tri.gameObject.SetActive(false);
        handL_Tal.gameObject.SetActive(false);
        handR_Col.gameObject.SetActive(false);
        handR_Tri.gameObject.SetActive(false);
        handR_Tal.gameObject.SetActive(false);

        headMoving = false;
        headRotation = 0.0f;
        canBeHit = false;
        inPhase1_5 = true;

        yield return null;
        anim.Play("Idle");
        yield return null;
        anim.Play("Phase2", -1, 0f);
        anim.SetLayerWeight(3, 1);
        yield return new WaitForSeconds(5.5f);

        canBeHit = true;
        headMoving = true;
        phase2Height = 1f;
    }
    private IEnumerator VictoryPose()
    {
        GameManager.preventDeathReset = true;
        headRotation = 0.0f;
        headMoving = false;
        phase2Height = 0.0f;
        lowerHeight = true;

        anim.Play("Victory");
        yield return new WaitForSeconds(9.2f);
        GameManager.preventDeathReset = false;
    }
    private void Victory()
    {
        StopAllCoroutines();
        StartCoroutine(VictoryPose());
    }
    public new IEnumerator PlayDeathShort()
    {
        if (!fightStarted)
            yield break;

        fightStarted = false;
        damage = 0;
        headMoving = false;
        headRotation = 0.0f;
        phase2Height = 0.0f;

        anim.Play("Defeat");
        yield return new WaitForSeconds(3.5f);
        CameraCtrl.instance.aud.Stop();
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < 20; i++)
        {
            rends[0].enabled = Random.Range(0,2) == 1;
            rends[1].enabled = Random.Range(0, 2) == 1;
            yield return new WaitForSeconds(0.1f);

            rends[0].enabled = Random.Range(0, 2) == 1;
            rends[1].enabled = Random.Range(0, 2) == 1;
            yield return new WaitForSeconds(0.05f);

            rends[0].enabled = Random.Range(0, 2) == 1;
            rends[1].enabled = Random.Range(0, 2) == 1;
            yield return new WaitForSeconds(0.1f);

            rends[0].enabled = true;
            rends[1].enabled = true;

            yield return new WaitForSeconds(1f - Mathf.Max(i * 0.1f, 0.1f));
        }

        Destroy(rends[0].transform.parent.gameObject);
        deathExplosionBoss = Instantiate(deathExplosionBoss);
        deathExplosionBoss.transform.position = transform.position;
        if (rend)
            rend.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        CameraCtrl.instance.aud.PlaySound(victoryAudioClip, true);
        CameraCtrl.instance.aud.loop = false;

        if (endStageAfterFight)
        {
            if (Player.instance != null)
                Player.instance.canBeHurt = false;
            yield return new WaitForSeconds(5.0f);

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
            Helper.GoToStage("Cutscene_Ending");
        }

        Destroy(gameObject);
    }


    public IEnumerator Behavior()
    {
        int diceRoll;
        bool isRight = true;
        phase = 1;
        while (true)
        {
            anim.Play("Idle");
            yield return null;

            if (Player.instance == null)
                Victory();

            // First Phase
            if (phase == 1)
            {
                for (int j = 0; j < Random.Range(2, 4); j++)
                {
                    if (health <= 42 && !inPhase1_5)
                        break;

                    if (Player.instance == null)
                        Victory();
                    for (int i = 0; i < Random.Range(1, 6); i++)
                    {
                        if (health <= 42 && !inPhase1_5)
                            break;

                        yield return Slap(isRight);
                        isRight = !isRight;
                    }
                    if (Player.instance == null)
                        Victory();
                    
                    diceRoll = Random.Range(0, 4);
                    switch (diceRoll)
                    {
                        case 0:
                        case 1:
                        case 2:
                            yield return Fist(isRight);
                            break;
                        case 3:
                            yield return DoubleFist(isRight);
                            break;
                    }
                    isRight = !isRight;
                }

                if (Player.instance == null)
                    Victory();

                diceRoll = Random.Range(0, 2);
                switch (diceRoll)
                {
                    case 0:
                        yield return Slam();
                        break;
                    case 1:
                        yield return Grab();
                        break;
                }

                if (Player.instance == null)
                    Victory();

                if (health <= 42 && !inPhase1_5)
                    yield return StartPhase1_5();

            }
        }
    }
    public IEnumerator Behavior2()
    {
        yield return StartPhase2();
        bool isRight = true;
        int diceRoll;
        while (true)
        {
            for (int j = 0; j < Random.Range(1, 2); j++)
            {
                for (int i = 0; i < Random.Range(0, 3); i++)
                {
                    if (Player.instance == null)
                        Victory();
                    yield return Slap(isRight);
                    isRight = !isRight;
                }
                if (Player.instance == null)
                    Victory();

                diceRoll = Random.Range(0, 4);
                switch (diceRoll)
                {
                    case 0:
                        yield return Fist(isRight);
                        break;
                    case 1:
                    case 2:
                        yield return DoubleFist(isRight);
                        break;
                    case 3:
                        yield return RockSlam();
                        break;;
                }
                isRight = !isRight;
            }

            if (Player.instance == null)
                Victory();

            diceRoll = Random.Range(0, 3);
            switch (diceRoll)
            {
                case 0:
                    yield return Slam();
                    break;
                case 1:
                case 2:
                    yield return Grab();
                    break;
            }

            for (int i = 0; i < Random.Range(0, 3); i++)
            {
                if (Player.instance == null)
                    Victory();

                diceRoll = Random.Range(0, 2);
                switch (diceRoll)
                {
                    case 0:
                        yield return ShootAtPlayer(5, 0.5f);
                        yield return new WaitForSeconds(0.35f);
                        break;
                    case 1:
                        isRight = GameManager.playerPosition.x > transform.position.x;
                        yield return ShootAround(40, isRight, 0.05f, 70 + 15f * (1f - phase2Height), 30);
                        yield return new WaitForSeconds(0.4f);
                        yield return ShootAround(40, !isRight, 0.05f, 70 + 15f * (1f - phase2Height), 30);
                        yield return new WaitForSeconds(0.6f);
                        break;
                }
            }

        }
    }

    public IEnumerator Slap(bool right)
    {
        anim.Play(right ? "SlapR" : "SlapL");
        headMoving = false;
        headRotation = 0.0f;

        yield return new WaitForSeconds(0.5f);

        if (right)
            handL_Tri.gameObject.SetActive(true);
        else
            handR_Tri.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        headMoving = true;
        if (right)
            handL_Tri.gameObject.SetActive(false);
        else
            handR_Tri.gameObject.SetActive(false);
    }
    public IEnumerator Fist(bool right)
    {
        anim.Play(right ? "FistR" : "FistL");
        headMoving = false;
        headRotation = 0.0f;

        yield return new WaitForSeconds(0.56f);
        
        if (right)
            handL_Tal.gameObject.SetActive(true);
        else
            handR_Tal.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.54f);

        headMoving = true;
        if (right)
            handL_Tal.gameObject.SetActive(false);
        else
            handR_Tal.gameObject.SetActive(false);
    }
    public IEnumerator DoubleFist(bool right)
    {
        anim.Play(right ? "DoubleFistR" : "DoubleFistL");
        headMoving = false;
        headRotation = 0.0f;
        damage = 6;

        yield return new WaitForSeconds(0.3f);

        if (right)
            handL_Tal.gameObject.SetActive(true);
        else
            handR_Tal.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.7f);
        GameManager.ShakeCamera(0.4f, 2f, false);
        yield return new WaitForSeconds(0.4f);

        damage = 4;
        headMoving = true;
        if (right)
            handL_Tal.gameObject.SetActive(false);
        else
            handR_Tal.gameObject.SetActive(false);
    }
    public IEnumerator Slam()
    {
        anim.Play("Slam_1");
        handR_Tal.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        GameManager.ShakeCamera(0.3f, 2, false);
        yield return new WaitForSeconds(0.1f);
        anim.Play("Slam_2");
        handL_Tal.gameObject.SetActive(false);
        handR_Tal.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        GameManager.ShakeCamera(0.3f, 2, false);
        yield return new WaitForSeconds(0.3f);
        anim.Play("Slam_3");
        handR_Tal.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        GameManager.ShakeCamera(0.3f, 2, false);
        yield return new WaitForSeconds(1.3f);
        handL_Tal.gameObject.SetActive(false);
        handR_Tal.gameObject.SetActive(false);
    }
    public IEnumerator Grab()
    {
        bool grabbedPlayer = false;
        hitPlayer = false;
        anim.Play("Grab");
        damage = 0;

        headMoving = false;
        headRotation = 0f;

        handL_Tri.gameObject.SetActive(true);
        handR_Tri.gameObject.SetActive(true);

        float time = 0.6f;
        while (time > 0.0f)
        {
            if (hitPlayer && !grabbedPlayer)
            {
                if (Player.instance)
                {
                    Player.instance.SetGrabbed(true, true);
                    grabbedPlayer = true;
                }
            }

            time -= Time.deltaTime;
            yield return null;
        }


        if (grabbedPlayer)
        {
            yield return new WaitForSeconds(0.5f);

            if (Player.instance)
                Player.instance.transform.position = handL_Tri.transform.position - handL_Tri.transform.right * 20f;

            if (Player.instance != null && Player.instance.health <= 8f)
            {
                lowerHeight = true;
                anim.Play("GrabFinal");
                yield return new WaitForSeconds(0.95f);

                if (Player.instance != null)
                    Player.instance.Damage(8000f);

                GameManager.preventDeathReset = true;

                yield return new WaitForSeconds(9.0f);

                GameManager.preventDeathReset = false;
                yield return null;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    anim.Play("GrabSlam", -1, 0f);
                    yield return new WaitForSeconds(0.333f);
                    GameManager.ShakeCamera(0.2f, 2f, false);
                    if (Player.instance)
                        Player.instance.Damage(2f);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(1.5f);

            if (Player.instance)
            {
                Player.instance.SetGrabbed(false, false);
                Player.instance.transform.position = handL_Tri.transform.position - handL_Tri.transform.right * 20f;
            }

            if (Player.instance == null)
                Victory();
            yield return null;

            anim.Play("Laugh");
            yield return new WaitForSeconds(2.0f);
            
        }
        else
        {
            handL_Tri.gameObject.SetActive(false);
            handR_Tri.gameObject.SetActive(false);
            headMoving = true;
            yield return new WaitForSeconds(1.2f);
        }

        damage = 4;
        handL_Tri.gameObject.SetActive(false);
        handR_Tri.gameObject.SetActive(false);
    }
    public IEnumerator RockSlam()
    {
        headMoving = false;
        headRotation = 0.0f;

        anim.Play("RockSlam");
        yield return new WaitForSeconds(1.6f);
        GameManager.ShakeCamera(0.5f, 3f, false);
        yield return new WaitForSeconds(0.5f);

        GameObject rock;

        for (int i = 0; i < 30; i++)
        {
            Vector3 targetPos = transform.position + new Vector3(Random.Range(-140f, 140f), 160f, 0);

            rock = Instantiate(rockDrop);
            rock.transform.position = targetPos;
            rock.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

            yield return new WaitForSeconds(0.15f);
        }


        yield return new WaitForSeconds(0.5f);

        rock = Instantiate(rockDrop);
        rock.transform.position = transform.position + Vector3.up * 160f;
        rock.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));

        yield return new WaitForSeconds(0.85f);
        rock.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-50f, 50f), -900f);
        anim.Play("RockSlamOuch");
        Damage(1f, true);
        canBeHit = false;

        yield return new WaitForSeconds(5f);

        canBeHit = true;
    }

    public IEnumerator ShootAround(int shots, bool startRight, float delay, float range = 60, int stopAtShot = 1000)
    {
        headMoving = false;
        headRotation = startRight ? 1f : -1f;
        float step = 2f / (shots - 1) * (startRight ? -1 : 1);
        
        anim.Play("Shoot", -1, 0f);

        for (int i = 0;  i < shots; i++)
        {
            if (delay >= 0.1f)
                anim.Play("Shoot", -1, 0f);

            Vector3 dir = Quaternion.Euler(0, 0, range * headRotation) * Vector3.down;
            GameObject shot = Shoot(mouthBone.position - mouthBone.up * 30f, dir, 3, 300, chargedShot);
            shot.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir));


            yield return new WaitForSeconds(delay);

            if (i == stopAtShot)
                break;

            if (i != shots - 1)
                headRotation += step;
        }
    }
    public IEnumerator ShootAtPlayer(int shots, float delay)
    {
        headMoving = false;

        yield return null;

        for (int i = 0; i < shots + 1; i++)
        {
            float distance = Mathf.Sin(Vector2.SignedAngle(Vector2.down, GameManager.playerPosition - mouthBone.parent.parent.position) * Mathf.Deg2Rad);
            headRotation = distance;
            anim.Play("Shoot", -1, 0f);

            yield return null;

            if (i > 0)
            {
                Vector3 dir = (GameManager.playerPosition - mouthBone.position + mouthBone.up * 30f).normalized;
                GameObject shot = Shoot(mouthBone.position - mouthBone.up * 30f, dir, 3, 300, charged2Shot);
                shot.transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir));
            }

            yield return new WaitForSeconds(delay);

        }
    }



}
