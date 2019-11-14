using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used Suzies, or Octupus Batteries.
/// They peacefully go up and down or left and right, then wait.
/// </summary>
public class En_Suzy : Enemy {


    // Keeps track of the movement speed, delay after each trip
    // and if the Suzy should move vertically or horizontally.
    public float speed = 16;
    public float waitTime = 10;
    public bool vertical = false;
    private float startTime;

    private bool moving = true;
    private bool movingForward = true;
    private Vector2 moveDir;
    private Vector2 moveDirRight;

    // Suzies only have three frames, so instead of using an animator,
    // their sprites are just changed through script.
    public Sprite eyeOpen;
    public Sprite eyeMiddle;
    public Sprite eyeClosed;


    protected override void Start()
    {
        base.Start();
        // Gets the Sprite Renderer, as the script needs to change the sprite manual.
        rend = GetComponentInChildren<SpriteRenderer>();
        if (!rend)
        {
            Debug.LogWarning("There is no Sprite Renderer in Suzy (" + name + ") or any of its children!");
        }

        // Sets the starting direction. It's messy, but it works.
        if ((vertical && Physics2D.OverlapPoint(transform.position + Vector3.up * 16.0f, 1 << 8)) ||
           (!vertical && Physics2D.OverlapPoint(transform.position + Vector3.right * 16.0f, 1 << 8)))
        {
            if (vertical)
            {
                moveDir = Vector2.down;
                moveDirRight = Vector2.left;
            }
            else
            {
                moveDir = Vector2.left;
                moveDirRight = Vector2.up;
            }
        }
        else
        {
            if (vertical)
            {
                moveDir = Vector2.up;
                moveDirRight = Vector2.right;
            }
            else
            {
                moveDir = Vector2.right;
                moveDirRight = Vector2.down;
            }
        }
        startTime = Time.time;
    }
    private void Update()
    {
        // Does nothing if the Player can't move yet.
        if (!GameManager.roomFinishedLoading)
            return;

        // Changes phase every time the cycle changes.
        if (((Time.time - startTime) % waitTime * 2 > waitTime && !movingForward) ||
             (Time.time - startTime) % waitTime * 2 < waitTime && movingForward)
        {
            movingForward = !movingForward;
            moveDir *= -1;
            moving = true;
            StartCoroutine(PlayEyeAnimation(true));
        }

        if (moving)
        {
            body.velocity = moveDir * speed;

            // When it hits a wall, it stops and shuts its eye.
            if (Physics2D.OverlapArea((Vector2)transform.position + moveDir * 8.0f + moveDirRight * 8.0f,
                                      (Vector2)transform.position + moveDir * 8.0f - moveDirRight * 8.0f,
                                      1 << 8))
            {
                moving = false;
                StartCoroutine(PlayEyeAnimation(false));
                body.velocity = Vector2.zero;
            }
        }
    }

    // Small animations are done through IEnumerators, just to avoid using an animator for something small like this
    private IEnumerator PlayEyeAnimation(bool open)
    {
        yield return new WaitForSeconds(0.2f);
        rend.sprite = eyeMiddle;
        yield return new WaitForSeconds(0.2f);
        rend.sprite = open ? eyeOpen : eyeClosed;
    }

}
