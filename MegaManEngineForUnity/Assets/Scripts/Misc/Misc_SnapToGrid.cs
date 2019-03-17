using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Snaps an object to a grid for this snappy pixelly feeling.
/// If the object is moving, this works best if the object is parented to some gameObject which is moving,
/// instead of this gameObject being in charge of movement itself.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Snap To Grid")]
public class Misc_SnapToGrid : MonoBehaviour {



    public Vector3 parentOffset = Vector3.zero;
    public SpriteRenderer sprite;

    public bool ignoreParent = true;

    private void LateUpdate()
    {
        SnapToPixelGrid();
    }


    public void SnapToPixelGrid()
    {
        if (sprite == null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }


        Vector3 disp = Vector3.zero;
        if (sprite != null && sprite.sprite != null)
        {
            disp = new Vector3(sprite.sprite.pivot.x - (int)sprite.sprite.pivot.x,
                               sprite.sprite.pivot.y - (int)sprite.sprite.pivot.y,
                               0);
            disp = Vector2.Scale(disp, new Vector2(transform.lossyScale.x > 0 ? 1 : -1, transform.lossyScale.y > 0 ? 1 : -1));
        }

        if (transform.parent == null || ignoreParent)
        {
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
        }
        else
        {
            transform.position = transform.parent.position + parentOffset + disp;
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z)) + disp;
        }
    }


}
