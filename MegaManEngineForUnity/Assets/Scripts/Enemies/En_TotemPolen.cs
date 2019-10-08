using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_TotemPolen : Enemy
{

    public Sprite spriteIdle;
    public Sprite[] spriteShoot;

    protected override void Start()
    {
        base.Start();
        rend = GetComponentInChildren<SpriteRenderer>();

        StartCoroutine(Behavior());
    }


    public IEnumerator Behavior()
    {
        while (true)
        {
            yield return null;

            transform.localScale = new Vector3(GameManager.playerPosition.x > transform.position.x ? -1 : 1, 1, 1);

            rend.sprite = spriteIdle;

            yield return new WaitForSeconds(2.0f);

            for (int i = 0; i < 4; i++)
                yield return Shoot(Random.Range(1, 5));

            yield return new WaitForSeconds(0.25f);

            rend.sprite = spriteIdle;
            yield return new WaitForSeconds(1.0f);
        }
    }


    public IEnumerator Shoot(int head)
    {
        if (head <= 0 || head > 4)
            yield break;

        yield return null;


        rend.sprite = spriteShoot[head - 1];

        yield return new WaitForSeconds(0.125f);

        Vector3 shootDir = transform.right * (transform.localScale.x > 0 ? -1 : 1);
        Shoot(transform.position + transform.up * 42f - transform.up * 16f * head, shootDir, 2, 200);

        yield return new WaitForSeconds(0.375f);
    }

}
