using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bomb Man uses this for an attack I think.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Link Spawn Objects")]
public class Misc_LinkSpawnObjects : MonoBehaviour
{

    public GameObject objToSpawn;

    public int objsPerSide = 4;
    public float distance = 8;
    public float startDistance = 8;
    public float delay = 0.25f;

    private void Start()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        GameObject expl;
        Vector3 targetPos;
        bool spawnLeft = true;
        bool spawnRight = true;

        for (int i = 0; i < objsPerSide; i++)
        {
            targetPos = transform.position + transform.right * (distance * i + startDistance);
            if (!Physics2D.Linecast(targetPos, targetPos - transform.up * 8, 1 << 8) ||
                Physics2D.Linecast(targetPos + transform.up * 8, targetPos + transform.up * 16, 1 << 8))
                spawnRight = false;

            if (spawnRight)
            {
                expl = Instantiate(objToSpawn);
                expl.transform.position = targetPos;
                expl.transform.rotation = transform.rotation;
                expl.transform.localScale = transform.localScale;
            }

            targetPos = transform.position - transform.right * (distance * i + startDistance);
            if (!Physics2D.Linecast(targetPos, targetPos - transform.up * 16, 1 << 8) ||
                Physics2D.Linecast(targetPos + transform.up * 8, targetPos + transform.up * 16, 1 << 8))
                spawnLeft = false;

            if (spawnLeft)
            {
                expl = Instantiate(objToSpawn);
                expl.transform.position = targetPos;
                expl.transform.rotation = transform.rotation;
                expl.transform.localScale = transform.localScale;
            }


            yield return new WaitForSeconds(delay);
        }
    }


}
