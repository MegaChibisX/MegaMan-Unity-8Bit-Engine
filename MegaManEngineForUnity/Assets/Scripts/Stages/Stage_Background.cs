using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a simple script that handles background parallax.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Background")]
public class Stage_Background : MonoBehaviour {

    // The player or the object the camera follows.
    public Transform followTarget;
    // Asks if the background should be moved in the X and Y axes.
    public bool X, Y;
    // The speed that the background moves at.
    public Vector2 speed;
    // Half the size of one full image.
    public Vector2 halfSize;
    // The offset of the background. It is recommended that the offset is 0 at any axis that moves.
    public Vector2 offset;


    // All of this technical which makes background parallax possible.
    private void Start()
    {
        //halfSize.x *= (1 + speed.x * 2);
        //halfSize.y *= (1 + speed.y * 2);
        halfSize.x *= 2;
        halfSize.y *= 2;
    }
    private void LateUpdate()
    {
        if (followTarget == null)
        {
            Debug.LogWarning("No target for the background " + name + " to follow");
            return;
        }

        Vector3 targetPos = followTarget.position * speed;
        targetPos.x = targetPos.x % halfSize.x;
        targetPos.y = targetPos.y % halfSize.y;
        Vector2 disp = followTarget.position;
        disp.x = disp.x % halfSize.x;
        disp.y = disp.y % halfSize.y;
        disp = followTarget.position - (Vector3)disp;

        targetPos = new Vector3(targetPos.x + disp.x + offset.x, targetPos.y + disp.y + offset.y, 0);
        transform.position = new Vector3(X ? targetPos.x : transform.position.x, Y ? targetPos.y : transform.position.y, transform.position.z);

    }

}
