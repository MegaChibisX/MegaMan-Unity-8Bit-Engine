using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnWp_WaveShot : Enemy
{


    public Vector3 origin;
    public Vector3 direction;

    public float moveSpeed = 300f;
    public float waveCycle = 1.0f;
    public float waveLength = 32f;

    private float lifeTime;


    protected override void Start()
    {
        base.Start();

        origin = transform.position;
        direction = Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward) * Vector3.right;
        direction.x *= transform.localScale.x;
        direction.y *= transform.localScale.y;
    }

    private void FixedUpdate()
    {
        lifeTime += Time.deltaTime;

        body.position = origin +
                            direction * moveSpeed * lifeTime +
                            Quaternion.AngleAxis(90f, Vector3.forward) * direction * Mathf.Sin(lifeTime * 180f * Mathf.Deg2Rad / waveCycle) * waveLength;
    }


}
