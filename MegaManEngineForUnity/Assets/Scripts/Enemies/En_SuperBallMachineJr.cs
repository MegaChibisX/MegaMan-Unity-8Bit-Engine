using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used by the Super Ball Machine Jr enemy, as the name implies.
/// This enemy simply shoots bouncing shots at an angle.
/// </summary>
public class En_SuperBallMachineJr : Enemy
{

    // The enemy only uses two sprites, so instead of using an animator,
    // the sprite is just changed after each shot for a fraction of a second.
    public Sprite spriteIdle;
    public Sprite spriteShot;

    // The shot the cannon shoots.
    public GameObject shot;

    // How often the cannon shoots, as well as an internal timer
    // that keeps track how much time is left until the next shot.
    public float shootIntervals = 2.0f;
    private float _shootInterval = 0.0f;


    protected override void Start()
    {
        // Sets up the enemy.
        base.Start();
        rend = GetComponentInChildren<SpriteRenderer>();
        if (rend == null)
        {
            Debug.LogWarning("There is no sprite renderer on SuperBallMachineJr named " + name + "!");
        }

        _shootInterval = shootIntervals;
    }

    protected void Update()
    {
        // Lowers the internal timer.
        _shootInterval -= Time.deltaTime;

        // If it's time to shoot, shots and resets the timer.
        if (_shootInterval <= 0.0f)
        {
            ShootBall();
            rend.sprite = spriteShot;
            _shootInterval = shootIntervals;
        }

        // Sets the appropriate sprite.
        if (shootIntervals - _shootInterval > 0.15f && rend.sprite == spriteShot)
        {
            rend.sprite = spriteIdle;
        }

    }

    protected void ShootBall()
    {
        // If there is no shot, you can't shoot.
        if (shot == null)
        {
            Debug.LogWarning("There is no shot in SuperBallMachineJr named " + name + "!");
            return;
        }

        // Shoots shot, sets its appropriate position and velocity.
        GameObject ball = Instantiate(shot);
        ball.transform.position = transform.position - transform.right * transform.localScale.x * 8.0f + transform.up * 10.0f;
        if (ball.GetComponent<Rigidbody2D>())
        {
            ball.GetComponent<Rigidbody2D>().velocity = (transform.up * 10.0f - transform.right * transform.localScale.x * 8.0f).normalized * 180.0f;
        }
    }


}
