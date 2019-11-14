using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MegaMan/Allies/X")]
public class X : Player
{

    // Checks if there is a wall to the left of the player.
    protected bool isWallSliding
    {
        get
        {
            return !isGrounded && Physics2D.Linecast(transform.position, transform.position + right * width * 1.1f * Mathf.Abs(input.x), 1 << 8);
        }
    }


    protected override void HandleInput_Movement()
    {
        // Wall jumps if needed.
        if (Input.GetButtonDown("Jump"))
        {
            if (isWallSliding)
            {
                StartCoroutine(WallJump());
            }
        }
        base.HandleInput_Movement();
    }
    protected override void HandlePhysics_Movement()
    {
        base.HandlePhysics_Movement();

        // Limits the y velocity when wall sliding.
        if (isWallSliding)
        {
            float lmt = -50 / Time.timeScale * timeScale;
            if (body.velocity.y * (gravityInverted ? -1 : 1) < lmt)
                body.velocity = new Vector2(body.velocity.x, lmt * (gravityInverted ? -1 : 1));
        }
    }
    protected IEnumerator WallJump()
    {
        // Sets the x and y velocity.
        body.velocity = new Vector2(-right.x * moveSpeed * 2f, jumpForce * timeScale / (GameManager.globalTimeScale != 0 ? GameManager.globalTimeScale : 1) * up.y);
        if (Input.GetButton("Slide"))
            dashing = true;
        float time = 0.1f;
        while (time > 0.0f)
        {
            // The player will move based on the input variable.
            // This way, there is no need to modify the canMove variable,
            // which can cause issues if it's changed on a timer.
            if (canMove && Time.timeScale > 0)
            {
                input.x = -right.x;
                // Updates the y velocity if canMove.
                body.velocity = new Vector2(-right.x * moveSpeed * 2f * timeScale / Time.timeScale, body.velocity.y);
            }
            else
                input.x = 0;

            time -= deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void Animate()
    {
        if (!canAnimate)
            return;

        // Plays the wall slide animation if needed. If not, animates like normal.
        if (isWallSliding)
        {
            string nameSuffix = "";

            if (shootTime > 0.0f)
                nameSuffix = "Shoot";
            else if (throwTime > 0.0f)
                nameSuffix = "Throw";

            anim.Play("Wall" + nameSuffix);
        }
        else
            base.Animate();
    }


}
