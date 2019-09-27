using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_Bomb : Enemy
{

    public float fuseTime = 10f;
    public bool destroyOnSolid = false;

    private void Update()
    {
        fuseTime -= Time.deltaTime;
        if (fuseTime <= 0.0f)
            Detonate();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnSolid)
            Detonate();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8 && destroyOnSolid)
            Detonate();
    }

    public virtual void Detonate()
    {
        deathExplosion = Instantiate(deathExplosion);
        deathExplosion.transform.position = transform.position;

        Enemy en;
        if ((en = deathExplosion.GetComponent<Enemy>()) != null)
            en.damage = damage;
        Destroy(gameObject);
    }

}
