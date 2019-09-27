using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_Skullmet : Enemy
{

    private Animator anim;


    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();

        StartCoroutine(Behavior());
    }


    public IEnumerator Behavior()
    {
        while (true)
        {
            yield return null;

            // Waits for a few seconds.
            anim.Play("Idle");
            yield return new WaitForSeconds(1.0f);

            // Prepares to shoot.
            anim.Play("Blink");
            yield return new WaitForSeconds(0.25f);

            anim.Play("Shoot");
            yield return new WaitForSeconds(0.125f);

            // Shoots, tells the bullet to use gravity and makes it drop towards the player's position.
            Vector3 shotOrigin = transform.position + transform.right * (transform.localScale.x > 0 ? -1 : 1) * 11 + transform.up * 11;
            Vector3 vel = Helper.LaunchVelocity(shotOrigin, GameManager.playerPosition + Vector3.up * 16f, 70, Physics2D.gravity.y * 100);

            GameObject shot = Shoot(shotOrigin, vel.normalized, 2, vel.magnitude);
            shot.GetComponent<Rigidbody2D>().gravityScale = 100;
        }
    }

}
