using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script for the Gemini Shot in-game.
/// Each shot is just a fragment, and a regular Gemini Laser
/// consists of three fragments.
/// 
/// For the Player Weapon Data that controls the player's colors
/// and what and how the player shoots, go to Pl_WeaponData.cs.
/// </summary>
public class PlWp_GeminiShot : Pl_Weapon {

    // Regular Gemini Lasers have two possible sprites,
    // the horizontal one they start with, and the diagonal
    // one they change to after they hit a wall.
    // The Double Gear Gemini Laser just has no direction
    // and is a simple sphere. 
    public Sprite horizontal;
    public Sprite diagonal;
    public Sprite variable;
    private SpriteRenderer rend;

    [System.NonSerialized]
    public AudioClip geminiNoise;
    private float shotRecoverTime = 0.0f;

    // Only the first fragment of a Gemini Laser
    // should make a sound, to prevent the *annoying*
    // noise from getting worse.
    [System.NonSerialized]
    public int shotOrder;
    // Gemini Lasers should get destroyed
    // after a few bounces.
    public int bouncesLeft = 10;


    // The direction and speed of a Gemini Laser,
    // as well as if it should only move diagonally
    // when it bounces from a solid surface.
    public Vector2 moveDir = Vector2.right;
    public float speed = 100.0f;
    public bool allowAnyAngle = false;

    public override void Start()
    {
        base.Start();

        // The Gemini Laser doesn't need to use an animator,
        // as it only uses one sprite in each phase.
        rend = GetComponentInChildren<SpriteRenderer>();
        if (!rend)
        {
            Debug.LogWarning("There is no Sprite Rendered in Gemini Shot (" + name + ")!");
        }

        // Finds the appropriate angle.
        if (allowAnyAngle)
            moveDir = transform.right;
        else
            moveDir = new Vector2(rend.flipX ? 1 : -1, 0);
    } 
    public void FixedUpdate()
    {
        // Sets the velocity of the Gemini Laser.
        body.velocity = moveDir * speed;

        // If there is a wall in front of the laser, it bounces.
        RaycastHit2D hit;
        if ((hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + moveDir * 8, 1 << 8)).collider != null)
        {
            // To prevent sound spam, there is a small delay after
            // a shot gets reflected, when the sound can't be played again.
            if (shotOrder == 0 && shotRecoverTime <= 0.0f)
            {
                Helper.PlaySound(geminiNoise);
                shotRecoverTime = 0.5f;
            }

            // Sets the right angle and orientation for the laser
            // once it hits a solid surface.
            if (allowAnyAngle)
            {
                moveDir = Quaternion.AngleAxis(Vector2.SignedAngle(hit.normal, -moveDir) * 2, Vector3.forward) * moveDir;
                transform.position = hit.point + moveDir * 6;
            }
            if (moveDir.y == 0)
            {
                if (!rend.flipX)
                    moveDir = new Vector2(1, 1).normalized;
                else
                    moveDir = new Vector2(-1, 1).normalized;
                rend.flipX = !rend.flipX;
                rend.sprite = diagonal;
                transform.position = hit.point + moveDir * 6;
            }
            else
            {
                rend.flipX = !rend.flipX;
                if (Physics2D.OverlapPoint((Vector2)transform.position + new Vector2(moveDir.x, 0) * 8, 1 << 8))
                    moveDir = new Vector2(-moveDir.x, moveDir.y).normalized;
                else
                    moveDir = new Vector2(moveDir.x, -moveDir.y).normalized;
                transform.position = hit.point + moveDir * 6;
            }

            // Lowers the remaining bounces for the laser
            // and destroyes it if needed.
            bouncesLeft--;
            if (bouncesLeft <= 0)
                Destroy(gameObject);
        }

        // Lowers the shotRecoverTime timer.
        if (shotRecoverTime > 0)
            shotRecoverTime -= Time.deltaTime;
    }

}
