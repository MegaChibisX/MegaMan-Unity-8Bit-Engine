using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_TrackBullet : Enemy
{


    public float speed = 50f;

    public bool hor = true;
    public bool ver = true;

    public float dirAngleRate = 360f;

    private Vector3 moveDir;


    private void FixedUpdate()
    {
        Vector3 targetDir = GameManager.playerPosition - transform.position;
        if (!hor)
            targetDir.x = 0;
        if (!ver)
            targetDir.y = 0;

        targetDir.Normalize();

        moveDir = Vector3.RotateTowards(moveDir, targetDir, dirAngleRate * Time.fixedDeltaTime * Mathf.Deg2Rad, 1000f);

        body.velocity = moveDir * speed;
        
    }

}
