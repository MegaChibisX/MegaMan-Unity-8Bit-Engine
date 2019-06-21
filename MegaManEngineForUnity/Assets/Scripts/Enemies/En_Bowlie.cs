using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_Bowlie : Enemy
{

    [Header("--------")]
    public Sprite idle1;
    public Sprite idle2;
    private SpriteRenderer rend;

    public EnWp_BowlieBall ball;

    private float time = 0.0f;
    public bool facesLeft = true;

    public float speed = 50.0f;


    protected override void Start()
    {
        base.Start();
        rend = GetComponentInChildren<SpriteRenderer>();

        if (GameManager.playerPosition.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);

        facesLeft = transform.localScale.x > 0;
    }

    protected void FixedUpdate()
    {
        // Moves the Bowlie
        body.MovePosition(transform.position + transform.right * -speed * Time.fixedDeltaTime * transform.localScale.x);

        // Drops the ball if the player is close to the bowlie.
        if ((GameManager.playerPosition.x - transform.position.x) * (facesLeft ? -1 :1) < 16 && ball != null)
        {
            ball.Activate();
            ball = null;
        }
    }

    protected void LateUpdate()
    {
        // Plays the propeller animation based on a timer. No need for an Animator for such simple animations.
        time += Time.deltaTime;
        if (time > 0.2f)
            time %= 0.2f;

        if (time < 0.1f)
            rend.sprite = idle1;
        else
            rend.sprite = idle2;
    }


}
