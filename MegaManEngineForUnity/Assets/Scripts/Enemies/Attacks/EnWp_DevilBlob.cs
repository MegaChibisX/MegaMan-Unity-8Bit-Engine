using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_DevilBlob : Enemy
{

    public Collider2D col;
    public Enemy target;
    public Transform eye;
    public Vector3 startPos;

    public Vector2 direction = Vector2.zero;
    public float speed = 64;
    public bool moving = false;
    public float bounceCooldown = 0.0f;
    private float destroyTime = 0.0f;

    public Material[] eyeMat;
    public Material flashMat;
    public MeshRenderer[] eyeObjs;

    protected override void LateUpdate()
    {
        if (eyeObjs != null && eyeObjs.Length > 0 &&
            eyeMat != null  && eyeMat.Length > 0)
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
        }
        if (invisTime > 0.0f)
            invisTime -= Time.deltaTime;
    }

    public override void Damage(Pl_Weapon weapon)
    {
        if (target && invisTime <= 0.0f)
            target.Damage(weapon.damage * 0.33f, false);
        base.Damage(weapon);
    }
    private void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, direction));

        if (bounceCooldown > 0.0f)
            bounceCooldown -= Time.deltaTime;

        if (destroyTime > 0.0f)
        {
            destroyTime -= Time.deltaTime;
            if (destroyTime <= 0.0f)
                Kill(false, false);
        }
    }
    private void FixedUpdate()
    {
        if (moving)
            body.velocity = direction.normalized * speed;
        else
            body.MovePosition(Vector2.MoveTowards(transform.position, startPos, 96f * Time.fixedDeltaTime));
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!moving)
            return;

        if (collision.collider.gameObject.layer == 8 && bounceCooldown <= 0.0f)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {

                direction = Vector2.Reflect(direction, collision.contacts[i].normal);
            }
            bounceCooldown = 0.1f;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!moving)
            return;

        if (collision.gameObject.layer == 11 && destroyTime <= 0.0f)
        {
            col.enabled = false;
            destroyTime = 1.0f;
        }
    }

    public void SetHurtability(bool value)
    {
        if (value)
        {
            eye.localScale = Vector3.one;
        }
        else
        {
            eye.localScale = Vector3.zero;
        }

        canBeHit = value;
    }


}
