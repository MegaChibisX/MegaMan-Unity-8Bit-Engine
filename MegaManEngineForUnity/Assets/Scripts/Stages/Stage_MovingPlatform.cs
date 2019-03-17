using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Moving platform.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Moving Platform")]
public class Stage_MovingPlatform : MonoBehaviour {


    // The position that the object is at when the room starts.
    private Vector2 startPos;
    // Which position the platform is heading towards.
    private int targetIndex;

    // These are the relative positions that the platform
    // will move towards, one by one.
    public Vector2[] points;
    // The speed of the platform.
    public float speed = 1.0f;

    private BoxCollider2D col;


    private void Start()
    {
        // Error check.
        if (points.Length == 0)
        {
            Debug.LogWarning("There are no points for the moving platform " + name + " to move!");
            return;
        }

        // Makes the positions be relative to the starting position of the platform.
        startPos = transform.position;
        transform.position = startPos + points[0];
        targetIndex = 0;
        col = GetComponentInChildren<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if (points.Length == 0)
            return;

        // Checks if the platform has reached the destination. More accurately, checks if the platform is very close, as we're dealing with floats, and you never know when 
        if (((Vector2)transform.position - points[targetIndex] - startPos).sqrMagnitude < 1.1f)
        {
            targetIndex++;
            if (targetIndex >= points.Length)
                targetIndex = 0;
            else if (targetIndex < 0)
                targetIndex = points.Length - 1;
        }

        // Moves towards the desired position.
        Vector2 disp = Vector2.MoveTowards(transform.position, startPos + points[targetIndex], speed * Time.fixedDeltaTime) - (Vector2)transform.position;
        foreach (Collider2D col in Physics2D.OverlapBoxAll((Vector2)transform.position + col.offset,
                 Vector3.Scale(col.size, transform.lossyScale) * 1.1f, transform.eulerAngles.z, 1 << 0))
        {
            if (col.attachedRigidbody)
                col.attachedRigidbody.position += new Vector2(disp.x, disp.y - 1);
        }

        transform.position += new Vector3(disp.x, disp.y, 0);
    }


    private void OnDrawGizmosSelected()
    {
        // Displays the positions the platform will visit,
        // and connects all the bordering ones.
        Vector2 pivot = startPos == Vector2.zero ? (Vector2)transform.position : startPos;

        if (points == null || points.Length == 0)
            return;
        if (points.Length == 1)
        {
            Gizmos.DrawSphere(pivot + points[0], 4f);
            return;
        }
        for (int i = 0; i < points.Length - 1; i++)
        {
            // Fun fact, you can do math with colors like you can do math with vectors.
            Gizmos.color = Color.blue * (points.Length - i) / points.Length + Color.yellow * i / points.Length;

            Gizmos.DrawSphere(pivot + points[i], 4f);
            Gizmos.DrawLine(pivot + points[i], pivot + points[i + 1]);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pivot + points[points.Length - 1], 4f);
        Gizmos.DrawLine(pivot + points[points.Length - 1], pivot + points[0]);

    }

}
