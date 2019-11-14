/*
    The animator is a mess. I was experimenting with various things. There are better ways to do this.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bo_HeartDevil : Boss
{

    public EnWp_DevilBlob blob;
    public EnWp_DevilBlob blob2;

    public Enemy bigShot;

    public Vector3 leftCorner;
    public Vector3 rightCorner;

    public ParticleSystem particles;

    // A 2D array would look much nicer, btw
    public SkinnedMeshRenderer bodyPiece;
    public Collider2D bodyCol;
    public Collider2D eyeCol;
    public Transform meshRootBone;

    public SkinnedMeshRenderer[] leftArmObj;
    public Collider2D[] leftArmCol;
    public int leftArmRem = 3;

    public SkinnedMeshRenderer[] rightArmObj;
    public Collider2D[] rightArmCol;
    public int rightArmRem = 3;

    public SkinnedMeshRenderer[] leftLegObj;
    public Collider2D[] leftLegCol;
    public int leftLegRem = 3;

    public SkinnedMeshRenderer[] rightLegObj;
    public Collider2D[] rightLegCol;
    public int rightLegRem = 3;

    public Material[] eyeMat;
    public Material flashMat;
    public MeshRenderer[] eyeObjs;

    private bool isDissolved = true;

    private Animator anim;
    private AudioSource aud;
    public BoxCollider2D col;

    public AudioClip healSound;

    protected override void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        rend = anim.GetComponent<SpriteRenderer>();
        aud = GetComponent<AudioSource>();

        anim.gameObject.SetActive(false);

        leftCorner += transform.position;
        rightCorner += transform.position;
    }
    protected override void LateUpdate()
    {
        if (invisTime % 0.2f > 0.07f)
        {
            for (int i = 0; i < eyeObjs.Length; i++)
                eyeObjs[i].material = flashMat;
        }
        else
        {
            for (int i = 0; i < eyeObjs.Length; i++)
                eyeObjs[i].material = eyeMat[i];
        }
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawSphere( leftCorner + transform.position, 4f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(rightCorner + transform.position, 4f);
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
        GameManager.bossesActive--;
        if (!isDissolved)
            StartCoroutine(Dissolve());
        StartCoroutine(PlayDeathShort());
        if (GameManager.bossesActive <= 0)
            CameraCtrl.instance.aud.Stop();

        if (GameManager.maxFortressStage == 1)
        {
            GameManager.maxFortressStage = 2;
            GameManager.playFortressStageUnlockAnimation = true;
        }
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


        // Creates the pieces that will drop down or come up from the ground.
        anim.gameObject.SetActive(true);
        anim.Play("Stand");
        //yield return null;

        for (int i = 2; i >= 0; i--)
        {
             leftArmObj[i].GetComponentInChildren<GenerateBakedMeshChild>().Generate();
            rightArmObj[i].GetComponentInChildren<GenerateBakedMeshChild>().Generate();
             leftLegObj[i].GetComponentInChildren<GenerateBakedMeshChild>().Generate();
            rightLegObj[i].GetComponentInChildren<GenerateBakedMeshChild>().Generate();

             leftArmObj[i].transform.GetChild(0).gameObject.SetActive(false);
            rightArmObj[i].transform.GetChild(0).gameObject.SetActive(false);
             leftLegObj[i].transform.GetChild(0).gameObject.SetActive(false);
            rightLegObj[i].transform.GetChild(0).gameObject.SetActive(false);

             leftArmObj[i].enabled = false;
            rightArmObj[i].enabled = false;
             leftLegObj[i].enabled = false;
            rightLegObj[i].enabled = false;
        }
        bodyPiece.GetComponentInChildren<GenerateBakedMeshChild>().Generate();
        bodyPiece.transform.GetChild(0).gameObject.SetActive(false);
        // Plays intro.
        yield return Reform();
        yield return new WaitForSeconds(0.5f);

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

            //yield return Jump(1);
            //LookAtPlayer();
            //yield return new WaitForSeconds(1.0f);


            //if (Random.Range(0, 2) == 0)
            //    yield return Slap();
            //else
            //    yield return AirSlap();


            //yield return Jump(1);
            //LookAtPlayer();
            //yield return new WaitForSeconds(1.0f);

            yield return Shoot();
            yield return new WaitForSeconds(1.5f);

            yield return Dissolve();
            LookAtPlayer();
            yield return MakeBlobs();
            yield return new WaitForSeconds(2.0f);
            transform.position = ((leftCorner + rightCorner) * 0.5f - transform.position).x > 0 ? rightCorner : leftCorner;
            LookAtPlayer();
            yield return Reform();
        }
    }

    public IEnumerator Jump(float forwardMulti)
    {
        LookAtPlayer();
        yield return null;

        body.velocity =  Vector3.up * 360f + anim.transform.forward * anim.transform.lossyScale.z * 300 * forwardMulti;
        anim.CrossFade("Jump", 0.1f);

        while (!isGrounded)
            yield return null;
        body.velocity = Vector3.zero;

        GameManager.ShakeCamera(0.35f, 2f, true);

        anim.CrossFade("Stand", 0.2f);
    }
    public IEnumerator Slap()
    {
        LookAtPlayer();
        yield return null;

        anim.CrossFade("Slap", 0.1f, -1, 0f);
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(1f);
    }
    public IEnumerator AirSlap()
    {
        LookAtPlayer();
        yield return null;

        anim.CrossFade("Jump", 0.1f);
        body.velocity = Vector3.up * 360f;

        while (body.velocity.y > 0.0f)
            yield return null;

        body.velocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Kinematic;

        anim.CrossFade("AirSlap", 0.1f);
        yield return new WaitForSeconds(1f);

        body.velocity = Vector2.zero;
        body.bodyType = RigidbodyType2D.Dynamic;

        anim.CrossFade("Jump", 0.1f);

        while (!isGrounded)
            yield return null;

        anim.CrossFade("Stand", 0.1f);
        yield return new WaitForSeconds(0.5f);
    }
    public IEnumerator Shoot()
    {
        LookAtPlayer();

        anim.Play("Shoot");

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < 3; i++)
        {
            Shoot(eyeObjs[0].transform.position, (GameManager.playerPosition - eyeObjs[0].transform.position), 4, 350, bigShot.gameObject);
            yield return new WaitForSeconds(0.5f);
        }

        anim.Play("Stand");
        yield return new WaitForSeconds(0.125f);
    }

    public IEnumerator Dissolve()
    {
        bodyCol.enabled = false;
        eyeCol.enabled = false;
        foreach (MeshRenderer obj in eyeObjs)
            obj.enabled = false;

        particles.Play();

        for (int i = 2; i >= 0; i--)
        {
             leftArmCol[i].enabled = false;
            rightArmCol[i].enabled = false;
             leftLegCol[i].enabled = false;
            rightLegCol[i].enabled = false;

             leftArmObj[i].enabled = false;
            rightArmObj[i].enabled = false;
             leftLegObj[i].enabled = false;
            rightLegObj[i].enabled = false;

             leftArmObj[i].transform.GetChild(0).gameObject.SetActive(true);
            rightArmObj[i].transform.GetChild(0).gameObject.SetActive(true);
             leftLegObj[i].transform.GetChild(0).gameObject.SetActive(true);
            rightLegObj[i].transform.GetChild(0).gameObject.SetActive(true);

             leftArmObj[i].transform.localPosition = Vector3.zero;
            rightArmObj[i].transform.localPosition = Vector3.zero;
             leftLegObj[i].transform.localPosition = Vector3.zero;
            rightLegObj[i].transform.localPosition = Vector3.zero;

            StartCoroutine(DropPiece(leftArmObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(DropPiece(rightLegObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(DropPiece(rightArmObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(DropPiece(leftLegObj[i]));
            yield return new WaitForSeconds(0.125f);
        }


        yield return new WaitForSeconds(0.25f);
        StartCoroutine(DropPiece(bodyPiece, 0.75f));

        bodyCol.enabled = false;
        bodyPiece.enabled = false;
        bodyPiece.transform.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);
        particles.Stop();

        yield return new WaitForSeconds(1f);
        anim.enabled = true;
        isDissolved = true;
    }
    public IEnumerator Reform()
    {
        anim.enabled = true;
        anim.Play("Stand");
        yield return null;
        anim.enabled = false;
        particles.Play();

        StartCoroutine(SpawnPiece(bodyPiece));
        bodyPiece.enabled = false;
        bodyPiece.transform.GetChild(0).gameObject.SetActive(true);
        yield return new WaitForSeconds(0.375f);

        for (int i = 0; i < 3; i++)
        {
             leftArmObj[i].enabled = false;
            rightArmObj[i].enabled = false;
             leftLegObj[i].enabled = false;
            rightLegObj[i].enabled = false;

             leftArmObj[i].transform.GetChild(0).gameObject.SetActive(false);
            rightArmObj[i].transform.GetChild(0).gameObject.SetActive(false);
             leftLegObj[i].transform.GetChild(0).gameObject.SetActive(false);
            rightLegObj[i].transform.GetChild(0).gameObject.SetActive(false);


            StartCoroutine(SpawnPiece(leftArmObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(SpawnPiece(rightLegObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(SpawnPiece(leftLegObj[i]));
            yield return new WaitForSeconds(0.125f);
            StartCoroutine(SpawnPiece(rightArmObj[i]));
            yield return new WaitForSeconds(0.125f);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < 3; i++)
        {
             leftArmCol[i].enabled = true;
            rightArmCol[i].enabled = true;
             leftLegCol[i].enabled = true;
            rightLegCol[i].enabled = true;
        }

            anim.enabled = true;

        bodyCol.enabled = true;
        eyeCol.enabled = true;
        foreach (MeshRenderer obj in eyeObjs)
            obj.enabled = true;
        isDissolved = false;

        yield return new WaitForSeconds(1f);
        particles.Stop();
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator MakeBlobs()
    {
        List<EnWp_DevilBlob> blobs = new List<EnWp_DevilBlob>();
        List<bool> isHeart = new List<bool>();
        float right = -anim.transform.lossyScale.z;

        for (int x = 3; x >= 0; x--)
        {
            for (int y = 0; y < 5; y++)
            {
                EnWp_DevilBlob newBlob;
                if (Random.Range(0, 5) == 0)
                {
                    newBlob = Instantiate(blob2);
                    newBlob.target = this;
                    isHeart.Add(true);
                }
                else
                {
                    newBlob = Instantiate(blob);
                    isHeart.Add(false);
                }

                Vector3 offset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
                newBlob.transform.position = transform.position + offset + Vector3.right * right * (x - 1) * 26 + Vector3.down * 128;
                newBlob.startPos           = transform.position + offset + Vector3.right * right * (x - 1) * 26 + Vector3.up * 16 + Vector3.up * (-y + 2) * 28;
                newBlob.direction = new Vector2(right * 6, Random.Range(-24, 1)).normalized;
                newBlob.col.enabled = false;
                blobs.Add(newBlob);

                yield return new WaitForSeconds(0.05f + Random.Range(-0.01f, 0.01f));
            }
        }

        yield return new WaitForSeconds(4.0f);
        int blobCount = blobs.Count;

        for (int i = 0; i < blobCount; i++)
        {
            int index = Random.Range(0, Mathf.Min(blobs.Count - 1, 5));
            if (isHeart[index])
            {
                blobs[index].direction = (GameManager.playerPosition - blobs[index].transform.position).normalized;
                blobs[index].speed *= 1.3f;
            }

            blobs[index].col.enabled = true;
            blobs[index].moving = true;
            blobs.RemoveAt(index);
            isHeart.RemoveAt(index);

            yield return new WaitForSeconds(0.75f);
        }


    }

    public IEnumerator DropPiece(SkinnedMeshRenderer piece, float speedMulti = 1f)
    {
        piece.transform.localPosition = Vector3.zero;
        piece.rootBone = null;

        float speedDiv = 1f / speedMulti;
        float time = 1.5f * speedDiv;
        while (time > 0.0f)
        {
            piece.transform.localPosition = Vector3.down * (1.5f * speedDiv - time) * 128f * speedMulti;

            time -= Time.deltaTime;
            yield return null;
        }

        piece.transform.localPosition = Vector3.zero;
        piece.rootBone = meshRootBone;

        piece.enabled = false;
        piece.transform.GetChild(0).gameObject.SetActive(false);
    }
    public IEnumerator SpawnPiece(SkinnedMeshRenderer piece, float speedMulti = 1f)
    {
        piece.transform.GetChild(0).gameObject.SetActive(true);
        piece.transform.localPosition = Vector3.down * 1.5f * speedMulti;
        piece.rootBone = null;

        float speedDiv = 1f / speedMulti;
        float time = 1.5f * speedDiv;
        while (time > 0.0f)
        {
            piece.transform.localPosition = Vector3.down * time * 256f * speedMulti;

            time -= Time.deltaTime;
            yield return null;
        }

        piece.transform.localPosition = Vector3.zero;
        piece.rootBone = meshRootBone;

        piece.enabled = true;
        piece.transform.GetChild(0).gameObject.SetActive(false);
    }


    private bool isGrounded
    {
        get
        {
            return body.velocity.y <= 0 &&
                Physics2D.OverlapBox((Vector2)transform.position + Vector2.Scale(col.offset, col.transform.lossyScale * 0.1f) - (Vector2)transform.up,
                                     Vector2.Scale(new Vector2(col.size.x * 0.9f, col.size.y), col.transform.lossyScale), 0, 1 << 8);
        }
    }
    public override void LookAtPlayer()
    {
        // Looks at the player.
        if (GameManager.playerPosition.x > transform.position.x)
            anim.transform.localScale = new Vector3( 1, 1,-1);
        else
            anim.transform.localScale = new Vector3( 1, 1, 1);
    }

}
