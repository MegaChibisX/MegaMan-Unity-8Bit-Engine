using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        for (int i = 0; i < objsPerSide; i++)
        {
            expl = Instantiate(objToSpawn);
            expl.transform.position = transform.position + transform.right * (distance * i + startDistance);
            expl.transform.rotation = transform.rotation;
            expl.transform.localScale = transform.localScale;

            expl = Instantiate(objToSpawn);
            expl.transform.position = transform.position - transform.right * (distance * i + startDistance);
            expl.transform.rotation = transform.rotation;
            expl.transform.localScale = transform.localScale;


            yield return new WaitForSeconds(delay);
        }
    }


}
