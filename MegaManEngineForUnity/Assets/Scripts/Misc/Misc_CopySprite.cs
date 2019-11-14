using System;
using UnityEngine;

/// <summary>
/// Simple script that copies one Renderer's sprite to another Renderer.
/// "I'll be honest, I don't remember where this one is used."
///          -MegaChibisX, 2019
/// </summary>
[AddComponentMenu("MegaMan/Misc/Copy Sprite")]
public class Misc_CopySprite : MonoBehaviour {


    public SpriteRenderer rendToCopyFrom;
    public SpriteRenderer rendToUse;
    public string spritePath;
    private Sprite[] subSprites;

    private void Start()
    {
        if (!rendToUse)
            rendToUse = GetComponent<SpriteRenderer>();
        if (!rendToUse)
        {
            Debug.LogWarning("There is no Sprite Renderer in " + name + "!");
            Destroy(this);
        }
        if (!Resources.Load<Sprite>(spritePath))
        {
            Debug.LogWarning("There is no sprite in the resources with the name or path " + spritePath + "!\nAre you sure your sprite is in the Resources folder, and that the path is correct?");
        }
        subSprites = Resources.LoadAll<Sprite>(spritePath);
    }
    public void Reload()
    {
        if (!Resources.Load<Sprite>(spritePath))
        {
            Debug.LogWarning("There is no sprite in the resources with the name or path " + spritePath + "!\nAre you sure your sprite is in the Resources folder, and that the path is correct?");
        }
        subSprites = Resources.LoadAll<Sprite>(spritePath);
    }

    private void LateUpdate()
    {
        if (!rendToUse || !rendToCopyFrom)
        {
            Debug.Log("NO RENDERER");
            return;
        }

        if (!rendToCopyFrom.sprite || spritePath == "")
        {
            rendToUse.sprite = null;
            return;
        }

        Sprite newSprite = Array.Find(subSprites, item => item.name == rendToCopyFrom.sprite.name);
        if (newSprite)
            rendToUse.sprite = newSprite;

    }
}
