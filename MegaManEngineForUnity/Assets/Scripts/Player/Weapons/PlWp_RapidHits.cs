using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// It does multiple hits to everything it's in contact with. Used by the Black Hole Bomb and Black Hole Hole.
/// A DestroyAfterTime component is recommended, as the script continues does damaage forever.
/// </summary>
public class PlWp_RapidHits : Pl_Weapon
{

    // How often the damage is done. 0.25 is 4 hits every second.
    public float frequency;
    public AudioClip hitSound;
    public AudioSource hitSource;

    public Collider2D col;


    public override void Start()
    {
        base.Start();
        StartCoroutine(Hit());
    }


    private IEnumerator Hit()
    {
        while (true)
        {
            if (col != null)
            {
                // Disables and re-enables the collider in the next frame. That will register as a new contact, and damage again.
                col.enabled = false;
                yield return new WaitForFixedUpdate();
                col.enabled = true;
                hitSource.PlaySound(hitSound, true);
            }


            // Wait for [frequency] seconds.
            yield return new WaitForSecondsRealtime(frequency);
        }
    }

}
