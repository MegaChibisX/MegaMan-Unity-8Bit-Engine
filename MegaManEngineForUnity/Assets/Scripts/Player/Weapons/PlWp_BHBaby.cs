using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWp_BHBaby : Pl_Shot
{

    public float time = 1.0f;
    public GameObject blackHole;
    public float turnSpeed;
    private float curTurnSpeed;


    public override void Update()
    {
        base.Update();

        time -= Time.deltaTime;
        if (time <= 0.0f)
        {
            blackHole = Instantiate(blackHole);
            blackHole.transform.position = transform.position;

            Destroy(gameObject);
        }
    }
    public void FixedUpdate()
    {
        curTurnSpeed = Mathf.Lerp(curTurnSpeed, turnSpeed, Time.deltaTime * 0.2f);

        body.velocity = transform.right * transform.localScale.x * speed + transform.up * curTurnSpeed * Input.GetAxisRaw("Vertical");
    }

}
