using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWp_RapidHits : Pl_Weapon
{

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
                col.enabled = false;
                yield return new WaitForFixedUpdate();
                col.enabled = true;
                hitSource.PlaySound(hitSound, true);
            }


            yield return new WaitForSecondsRealtime(frequency);
        }
    }

}
