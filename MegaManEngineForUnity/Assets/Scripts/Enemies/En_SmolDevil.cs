using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_SmolDevil : Enemy
{

    private Animator anim;
    private SpriteRenderer rend;
    public BoxCollider2D col;

    public AudioClip shotSound;
    public AudioClip shot2Sound;

    public GameObject blobObj;
    public GameObject deblobObj;
    public GameObject shotObj;

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        rend = GetComponentInChildren<SpriteRenderer>();

        anim.Play("Idle");
        StartCoroutine(Behavior());
    }
    public override void Kill(bool makeItem, bool makeBolt)
    {
        // Makes either a big health or a big energy.
        GameObject item = Item.GetObjectFromItem(Item.GetRandomItem(0, 0, 1, 0, 1, 0, 0, 0));
        if (item)
        {
            item = Instantiate(item);
            item.transform.position = transform.position + center;
            item.transform.rotation = transform.rotation;
            item.transform.localScale = transform.localScale;
        }
        // Makes an 1-up
        item = Item.GetObjectFromItem(Item.GetRandomItem(0, 0, 0, 0, 0, 0, 0, 1));
        if (item)
        {
            item = Instantiate(item);
            item.transform.position = transform.position + center;
            item.transform.rotation = transform.rotation;
            item.transform.localScale = transform.localScale;
        }
        // Makes a few bolts
        for (int i = 0; i < 5; i++)
        {
            item = Item.GetObjectFromItem(Item.GetRandomBolt(0, 1, 1.6f, 0));
            if (item)
            {
                item = Instantiate(item);
                item.transform.position = transform.position + center;
                item.transform.rotation = transform.rotation;
                item.transform.localScale = transform.localScale;

                item.GetComponent<Rigidbody2D>().AddForce(transform.up * Random.Range(-15000, 15000f) + transform.right * Random.Range(0, 8000f));
            }
        }

        base.Kill(false, false);
    }


    private IEnumerator Behavior()
    {
        anim.Play("Idle");
        yield return new WaitForSeconds(2.0f);
        while (true)
        {
            if (Random.Range(0, 3) == 0)
                yield return Jump();
            if (Random.Range(0, 3) == 0)
                yield return Jump();

            anim.Play("Idle");
            yield return null;

            // Checks for distance from player.
            float time = 1.0f;
            bool playerNearby = false;

            while (time > 0.0f)
            {
                if (Mathf.Abs(GameManager.playerPosition.x - transform.position.x) < 128 &&
                    Mathf.Abs(GameManager.playerPosition.y - transform.position.y) < 32)
                {
                    playerNearby = true;
                    break;
                }

                time -= Time.deltaTime;
                yield return null;
            }

            // Flips and checks for distance again.
            if (!playerNearby)
            {
                yield return Flip();
                yield return Jump();
                anim.Play("Idle");
                for (int i = 0; i < 4; i++)
                {
                    yield return new WaitForSeconds(0.25f);
                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }

                if (Mathf.Abs(GameManager.playerPosition.y - transform.position.y) < 32)
                {
                    playerNearby = true;
                }
            }

            if (playerNearby)
            {
                yield return Split();
                anim.Play("Idle");
                yield return Shoot(3, 3, 45, 0.35f, 0f);
                yield return Shoot(5, 3, 27, 0.35f, 0f);
                yield return Shoot(4, 3, 33, 0.35f, 0f);
            }
            else
            {
                yield return Flip();
                yield return Shoot(7, 1, 25, 0.5f, 0, 0.1f);
            }

            yield return Flip();
            yield return Jump();

            anim.Play("Idle");
            yield return new WaitForSeconds(0.25f);

            yield return ShootTrack(3);

            anim.Play("Idle");
            yield return new WaitForSeconds(0.5f);
        }
    }

    private  IEnumerator Jump()
    {
        anim.Play("Idle");

        if (GameManager.playerPosition.x < transform.position.x)
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(-1, transform.localScale.y, 1);

        yield return new WaitForSeconds(0.15f);

        anim.Play("Jump");
        body.velocity = new Vector2(transform.localScale.x * -150f, transform.localScale.y * 300f);

        while (!isGrounded || body.velocity.y * (body.gravityScale > 0 ? 1 : -1) > 0)
        {
            body.velocity = new Vector2(transform.localScale.x * -150f, body.velocity.y);
            yield return null;
        }

        body.velocity = Vector2.zero;

        anim.Play("Land");
        yield return new WaitForSeconds(0.25f);
    }

    private IEnumerator Split()
    {
        Vector3 originPos = transform.position;
        Vector3 targetPos = GameManager.playerPosition;
        float distance = Mathf.Abs(targetPos.x - transform.position.x) + 78;
        float startGravity = body.gravityScale;

        RaycastHit2D hit;
        if (hit = Physics2D.BoxCast(transform.position + Vector3.Scale(col.offset, transform.localScale),
                             col.size, 0,
                             - transform.right * transform.localScale.x, distance + 9, 1 << 8))
            distance = hit.distance;
        float travelTime = distance / 200f;

        if (targetPos.x < transform.position.x)
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(-1, transform.localScale.y, 1);

        yield return new WaitForSeconds(0.25f);

        GameObject deblobInst = Instantiate(deblobObj);
        deblobInst.transform.position = transform.position;
        deblobInst.transform.localScale = transform.localScale;

        canBeHit = false;
        anim.Play("Idle");
        col.enabled = false;
        rend.enabled = false;
        body.gravityScale = 0f;
        transform.position = (transform.position - transform.right * transform.localScale.x * distance);

        Invoke("Reblob", travelTime + 0.7f);

        yield return new WaitForSeconds(0.72f);

        ShootBlob(originPos + _center + transform.up * 8f * transform.localScale.y, transform.localScale, travelTime);

        yield return new WaitForSeconds(0.4f);

        ShootBlob(originPos + _center - transform.up * 0f * transform.localScale.y, transform.localScale, travelTime);

        yield return new WaitForSeconds(0.45f);

        ShootBlob(originPos + _center - transform.up * 8f * transform.localScale.y, transform.localScale, travelTime);

        yield return new WaitForSeconds(travelTime);

        canBeHit = true;
        body.gravityScale = startGravity;

        if (!isGrounded)
        {
            anim.Play("Jump");

            while (!isGrounded)
                yield return null;

            anim.Play("Land");
            yield return new WaitForSeconds(0.25f);
        }
    }
    private void ShootBlob(Vector3 pos, Vector3 sca, float time)
    {
        GameObject obj = Instantiate(blobObj);
        obj.transform.position = pos;
        obj.transform.localScale = sca;
        obj.GetComponent<Misc_DestroyAfterTime>().time = time;
        obj.GetComponent<EnWp_Shot>().direction = new Vector2(transform.localScale.x > 0 ? -1 : 1, 0);
    }
    private void Reblob()
    {
        col.enabled = true;
        rend.enabled = true;
        anim.Play("Reblob");
    }

    private IEnumerator Flip()
    {
        anim.Play("Flip");
        body.gravityScale *= -1;

        yield return new WaitForSeconds(0.333f);

        transform.position += Vector3.up * 17 * transform.localScale.y;
        transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
        anim.Play("Jump");

        while (!isGrounded)
            yield return null;

        anim.Play("Land");
        yield return new WaitForSeconds(0.25f);
        anim.Play("Idle");
        yield return null;
    }

    private IEnumerator Shoot(int shots, int loops, float angle, float startDelay = 0.5f, float loopDelay = 0.35f, float shotDelay = 0.0f)
    {
        if (GameManager.playerPosition.x < transform.position.x)
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(-1, transform.localScale.y, 1);

        anim.Play("Shoot");
        yield return new WaitForSeconds(startDelay);

        if (loopDelay <= 0)
            loops = 1;
        for (int l = 0; l < loops; l++)
        {
            Vector3 shotDir = -transform.right * transform.localScale.x;
            shotDir = Quaternion.AngleAxis(-angle * (shots + 1) * 0.5f, Vector3.forward) * shotDir;

            if (shotDelay <= 0)
                Helper.PlaySound(shotSound);

            for (int s = 0; s < shots; s++)
            {
                shotDir = Quaternion.AngleAxis(angle, Vector3.forward) * shotDir;
                Shoot(transform.position + _center - transform.right * transform.localScale.x * 6, shotDir, 3, 200);

                if (shotDelay > 0)
                    Helper.PlaySound(shotSound);
                yield return new WaitForSeconds(shotDelay);
            }

            yield return new WaitForSeconds(loopDelay);
        }

    }
    private IEnumerator ShootTrack(int loops, float loopDelay = 0.2f)
    {
        if (GameManager.playerPosition.x < transform.position.x)
            transform.localScale = new Vector3(1, transform.localScale.y, 1);
        else
            transform.localScale = new Vector3(-1, transform.localScale.y, 1);

        anim.Play("Shoot");
        yield return new WaitForSeconds(1.5f);

        for (int l = 0; l < loops; l++) 
        {
            Vector3 origin = transform.position + _center - transform.right * transform.localScale.x * 6;
            Shoot(origin, GameManager.playerPosition - origin, 5, 300, shotObj);

            Helper.PlaySound(shot2Sound);
            yield return new WaitForSeconds(loopDelay);
        }
    }

    private bool isGrounded
    {
        get
        {
            return Physics2D.BoxCast(transform.position + _center, new Vector2(col.size.x, 1), 0,
                                     Vector3.down * transform.localScale.y, 20f, 1 << 8);
        }
    }
    private Vector3 _center
    {
        get
        {
            return Vector3.Scale(transform.localScale, center);
        }
    }

}
