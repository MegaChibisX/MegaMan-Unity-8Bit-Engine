using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class En_Press : Enemy
{

    [Header("--------")]

    public MeshFilter chainPiece;
    public Sprite chainSprite;

    public Vector3 startPos;
    public float maxHeight = 128;
    public float curHeight = 0;

    public float dropHeight = 64;
    public float raiseHeight = 32;

    public float waitDropped = 1.0f;
    public float waitRaised = 0.5f;
    public float waitTime = 0.0f;

    bool goingDown = true;


    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
    }

    private void Update()
    {

        // If the press needs to wait, waits.
        if (waitTime > 0.0f)
        {
            waitTime -= Time.deltaTime;
            return;
        }

        // Sets the height of the press.
        if (goingDown)
        {
            RaycastHit2D hit;
            curHeight = Mathf.Clamp(curHeight + dropHeight * Time.deltaTime, 0, maxHeight);
            if (curHeight >= maxHeight)
            {
                goingDown = !goingDown;
                waitTime = waitDropped;
            }
            else if (hit = Physics2D.Raycast(transform.position - transform.up * 16, -transform.up, 16, 1 << 8))
            {
                goingDown = !goingDown;
                waitTime = waitDropped;
                curHeight = Vector3.Project(hit.point - (Vector2)startPos, transform.up).magnitude - 32;
            }
        }
        else
        {
            curHeight = Mathf.Clamp(curHeight - raiseHeight * Time.deltaTime, 0, maxHeight);
            if (curHeight <= 0)
            {
                goingDown = !goingDown;
                waitTime = waitRaised;
            }
        }

        // Sets the position of the press.
        transform.position = startPos - transform.up * curHeight;


        // Sets the sprite of the chain to the appropriate height.
        Mesh newSprite = Helper.ExtendSprite(chainSprite, curHeight);
        chainPiece.mesh = newSprite;
    }

}
