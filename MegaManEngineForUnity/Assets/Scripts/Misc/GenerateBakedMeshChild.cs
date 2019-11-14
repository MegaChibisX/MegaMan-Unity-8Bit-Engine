using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBakedMeshChild : MonoBehaviour
{

    public void Generate()
    {
        SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();
        if (!rend)
            return;

        Transform[] tr = GetComponentsInChildren<Transform>();
        for (int i = 0; i < tr.Length; i++)
        {
            if (tr[i] != transform)
                Destroy(tr[i].gameObject);
        }

        Mesh mesh = new Mesh();
        rend.BakeMesh(mesh);

        GameObject child = new GameObject("Static Mesh");

        child.transform.parent = transform;
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one / transform.lossyScale.x;

        child.layer = gameObject.layer;

        child.AddComponent<MeshFilter>().mesh = mesh;
        child.AddComponent<MeshRenderer>().sharedMaterial = rend.sharedMaterial;
    }


}
