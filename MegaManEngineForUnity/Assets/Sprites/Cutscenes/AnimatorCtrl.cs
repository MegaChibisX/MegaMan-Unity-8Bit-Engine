using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorCtrl : MonoBehaviour
{

    
    public int curClip;
    private int prevClip;
    private Animator anim;

    void Start()
    {
        prevClip = curClip;
        anim = GetComponent<Animator>();
    }

	void Update()
    {
        if (curClip != prevClip)
        {
            anim.SetInteger("AnimClip", curClip);
            prevClip = curClip;
        }
	}
}
