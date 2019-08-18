using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MegaMan/Misc/Sprite Animation")]
public class Misc_SpriteAnimation : MonoBehaviour
{

    protected SpriteRenderer rend;
    public int frameSpeed = 12;
    public Sprite[] sprites;

    protected void Start()
    {
        rend = GetComponentInChildren<SpriteRenderer>();
    }
    protected virtual void LateUpdate()
    {
        if (sprites != null && rend != null && sprites.Length > 0.0f)
            rend.sprite = sprites[(int)(Time.time * frameSpeed) % sprites.Length];
    }
}
