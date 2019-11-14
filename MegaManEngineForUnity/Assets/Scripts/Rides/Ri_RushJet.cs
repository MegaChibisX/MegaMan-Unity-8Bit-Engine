using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// You can flyyy on a dog.
/// </summary>
public class Ri_RushJet : Ride
{

    private Rigidbody2D body;
    private BoxCollider2D col;
    private Animator anim;

    public override void Start()
    {
        base.Start();
        body = GetComponent<Rigidbody2D>();
        col = GetComponentInChildren<BoxCollider2D>();
        anim = GetComponentInChildren<Animator>();
        canBeRidden = false;

        StartCoroutine(Intro());
    }
    public override void Update()
    {
        if (rider != null)
        {
            // Moves up and down if there is a rider.
            body.velocity = new Vector2(rider.input.x * 64f, rider.input.y * 64f);
            if (rider.input.x != 0)
            {
                transform.localScale = new Vector3(rider.input.x > 0 ? 1 : -1, transform.localScale.y);
                rider.anim.transform.localScale = new Vector3(rider.input.x > 0 ? 1 : -1, transform.localScale.y);
            }
        }
        else
        {
            // Rush would fly off otherwise.
            body.velocity = Vector3.zero;
        }
    }

    public override void Mount()
    {
        base.Mount();
        if (rider)
        {
            rider.canAnimate = false;
            rider.anim.Play("Stand");
        }
    }
    public override void Dismount()
    {
        if (rider)
        {
            rider.canAnimate = true;
        }
        base.Dismount();
    }

    public void Kill()
    {
        GetComponentInChildren<Animator>().Play("Outro");
        Invoke("Kill2", 1f);
        canBeRidden = false;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            Destroy(col);
        }
    }
    private void Kill2()
    {
        Destroy(gameObject);
    }

    public IEnumerator Intro()
    {
        anim.Play("Intro");
        yield return new WaitForSeconds(1.3f);
        anim.Play("Jet");
        canBeRidden = true;
    }

}