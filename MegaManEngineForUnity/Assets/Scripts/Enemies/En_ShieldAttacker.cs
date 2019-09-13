using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_ShieldAttacker : Enemy
{

    public float speed = 100f;

    public float maxLeft = -16f;
    public float maxRight = 16f;

    private bool moving = true;
    private bool goingLeft = true;

    public SpritePair[] pairs;
    public SpritePair[] pairs2;
    public SpriteRenderer rendSource;
    public SpriteRenderer rendOutput1;
    public SpriteRenderer rendOutput2;

    private Animator anim;

    protected override void Start()
    {
        base.Start();

        goingLeft = GameManager.playerPosition.x < transform.position.x;
        if (!goingLeft)
            transform.localScale = new Vector3(-1, 1, 1);

        maxLeft += transform.position.x;
        maxRight += transform.position.x;

        anim = GetComponentInChildren<Animator>();
    }
    private void LateUpdate()
    {
        ChangeSpriteColor(rendSource, rendOutput1, pairs);
        ChangeSpriteColor(rendSource, rendOutput2, pairs2);
    }
    protected void FixedUpdate()
    {
        if (moving)
        {
            Vector3 targetPos = new Vector3(goingLeft ? maxLeft : maxRight, transform.position.y, transform.position.z);

            if ((targetPos - transform.position).sqrMagnitude < 0.1f)
                StartCoroutine(Turn());
            else
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * speed);
        }
    }
    public override void ChangeColorScheme(Color[] colors)
    {
        if (colors.Length >= 1)
        {
            rendOutput1.color = colors[0];
        }
    }

    protected void ChangeSpriteColor(SpriteRenderer source, SpriteRenderer output,  SpritePair[] spritePairs)
    {
        foreach (SpritePair s in spritePairs)
        {
            if (source.sprite == s.Key)
            {
                output.sprite = s.Value;
                return;
            }
        }
    }

    protected IEnumerator Turn()
    {
        moving = false;
        anim.Play("Turn");

        yield return new WaitForSeconds(0.3f);

        if (goingLeft)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3( 1, 1, 1);

        yield return new WaitForSeconds(0.2f);

        anim.Play("Idle");

        goingLeft = !goingLeft;
        moving = true;
    }


    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position + Vector3.right * maxLeft, 4f);
        Gizmos.DrawSphere(transform.position + Vector3.right * maxRight, 4f);
        Gizmos.DrawLine(transform.position + Vector3.right * maxLeft,
                        transform.position + Vector3.right * maxRight);
    }


}
