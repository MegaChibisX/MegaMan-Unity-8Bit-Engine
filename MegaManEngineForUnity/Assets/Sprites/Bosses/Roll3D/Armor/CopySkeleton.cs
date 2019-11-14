using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopySkeleton : MonoBehaviour {


    public SkinnedMeshRenderer source;
    public SkinnedMeshRenderer[] targets;

    private void Start()
    {
        SetTargetBonesToSource(false);


    }


    public void SetTargetBonesToSource(bool fromInspector)
    {
        if (source == null || targets == null)
        {
            Debug.LogWarning("You forgot to assign a source or target Copy Skeleton over " + name);
            return;
        }

        foreach (SkinnedMeshRenderer target in targets)
        {
            target.bones = source.bones;
            target.rootBone = source.rootBone;
            target.transform.parent = source.transform.parent;
        }

        if (!fromInspector)
            Destroy(this);
    }

}
