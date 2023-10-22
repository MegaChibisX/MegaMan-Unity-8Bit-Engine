using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Your friend and trampoline, Rush!
/// </summary>
public class PlWp_RushCoil : MonoBehaviour
{

    // How high the player jumps.
    public float jumpForce = 500f;
    // Can't jump multiple times.
    public bool madeContact = false;
    // Can't jump when playing intro/outro
    private bool canJump = false;

    public void Start()
    {
        // Play intro.
        GetComponentInChildren<Animator>().Play("Intro");
        Invoke("AllowJump", 1.150f);
    }

    public void UseCoil()
    {
        // Use the coil if not already used.
        if (madeContact || !canJump)
            return;

        GetComponentInChildren<Animator>().Play("Coil");
        // Die in 2 seconds.
        Invoke("Kill", 2f);
    }
    public void Kill()
    {
        GetComponentInChildren<Animator>().Play("Outro");
        // Die  again in 1 second.
        Invoke("Kill2", 1f);
        canJump = false;
    }
    private void Kill2()
    {
        // Die.
        Destroy(gameObject);
    }
    public void AllowJump()
    {
        canJump = true;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // Make the player jump if that was indeed the player.
        Rigidbody2D body = other.attachedRigidbody;
        if (!madeContact && body && body.GetComponent<Player>() && canJump)
        {
            UseCoil();
            madeContact = true;
			bool gravityFlip = body.GetComponent<Player>().gravityInverted;
            body.velocity = new Vector2(body.velocity.x, jumpForce * (gravityFlip ? -1 : 1));
        }
    }


}
