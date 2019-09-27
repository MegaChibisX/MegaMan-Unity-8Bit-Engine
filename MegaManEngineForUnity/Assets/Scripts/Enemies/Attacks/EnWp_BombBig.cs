using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_BombBig : EnWp_Bomb
{

    public int explosions = 4;
    public float startAngle = 0;
    public float startRadius = 8;
    public float anglePerExplosion;
    public float angleDrop = 0;
    public float radiusPerExplosion;
    public float delayPerExplosion;

    private bool detonated = false;

    public override void Detonate()
    {
        if (detonated)
            return;

        detonated = true;
        StartCoroutine(Kablewy());
    }

    private IEnumerator Kablewy()
    {
        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector2.zero;
        if (GetComponentInChildren<SpriteRenderer>())
            GetComponentInChildren<SpriteRenderer>().enabled = false;

        for (int i = 0; i < explosions; i++)
        {
            Vector3 origin = transform.position + Quaternion.AngleAxis(i * (anglePerExplosion - angleDrop * i) + startAngle, Vector3.forward) *
                                                        Vector2.right * (startRadius + radiusPerExplosion * i);
            GameObject expl = Instantiate(deathExplosion);
            expl.transform.position = origin;

            if (i % (explosions/4) != 0)
            {
                // Removes some of the noise.
                AudioSource source;
                if ((source = expl.GetComponent<AudioSource>()) != null)
                    Destroy(source);
            }

            Enemy en;
            if ((en = expl.GetComponent<Enemy>()) != null)
                en.damage = damage;

            yield return new WaitForSeconds(delayPerExplosion);
        }

        Destroy(gameObject);
    }


}
