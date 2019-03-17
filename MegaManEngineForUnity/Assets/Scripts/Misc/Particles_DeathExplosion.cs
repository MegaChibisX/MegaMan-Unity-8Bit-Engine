using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the death explosion of a Player or Boss.
/// Kinda crudely written, but it does a small job, and it does it well enough.
/// </summary>
[AddComponentMenu("MegaMan/Particles/Death Explosion")]
public class Particles_DeathExplosion : MonoBehaviour {

    // Explosions need to have a Rigidbody attached to move on their own.
    public Rigidbody2D explosionParticle;
    public AudioClip audioClip;

    public void Start()
    {
        // Can't have a big explosion without the explosive parts.
        if (!explosionParticle)
        {
            Debug.LogWarning("There is no explosion particle at " + name + "!");
        }

        Helper.PlaySound(audioClip);

        // Creates two rows of explosions at different rates.
        CreateRoundExplosion(0, 360, 12, 100);
        CreateRoundExplosion(0, 360,  8, 50);
    }


    // startAngle of 0 makes the first particle move to the right,
    // with 360 degrees being a full circle.
    // Usually a maxAngle of 360 and any startAngle will suffice.
    // shotCount is the number of explosions that will be equally
    // spread between the start and max angles.
    // speed is the speed of each shot.
    private void CreateRoundExplosion(float startAngle, float maxAngle, int shotCount, float speed)
    {
        // Finds the starting direction and the angle increment per shot.
        Vector2 direction = Quaternion.AngleAxis(startAngle, Vector3.forward) * Vector2.right;
        float anglePerShot = maxAngle / shotCount;

        // Creates each shot and increases the angle by the desired angle step.
        for (int i = 0; i < shotCount; i++)
        {
            Rigidbody2D b = Instantiate(explosionParticle.gameObject).GetComponent<Rigidbody2D>();
            b.velocity = direction * speed;

            b.transform.parent = transform;
            b.transform.position = transform.position;

            b.GetComponentInChildren<Animator>().Play("Looped");

            direction = Quaternion.AngleAxis(anglePerShot, Vector3.forward) * direction;
        }
    }


}
