using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Helper simply holds basic functions that need to be called from
/// many different objects.
/// </summary>
public static class Helper
{


    // Plays a sound from an AudioSource compoent.
    public static void PlaySound(this AudioSource source, AudioClip clip, bool forcePlay)
    {
        if (source == null || !source.enabled)
            return;

        if (clip == null)
        {
            source.Stop();
            return;
        }
        else if (source.clip != clip || !source.isPlaying || forcePlay)
        {
            source.clip = clip;
            source.time = 0;
            source.Play();
        }
    }

    // Plays a sound without using a specific AudioSource. Creates a new gameObject.
    public static void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        GameObject obj = new GameObject("Sound Clip (" + clip.name + ")");
        obj.AddComponent<Misc_DestroyAfterTime>().time = clip.length + 2f;
        AudioSource source = obj.AddComponent<AudioSource>();


        source.clip = clip;
        source.time = 0;
        source.Play();
    }


    // Calculates the trajectory of a projectile and returns the Launch Velocity.
    //Doesn't take drag into consideration, so it could stand to be improved.
    public static Vector3 LaunchVelocity(Vector3 origin, Vector3 target, float angle, float gravity)
    {
        Vector3 dir = target - origin;
        float h = dir.y;
        dir.y = 0;
        float dist = dir.magnitude;
        float a = Mathf.Abs(angle) * Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += h / Mathf.Tan(a);

        // If val was negative, it returned NaN,NaN,Nan as the velocity
        float val = dist * gravity / Mathf.Sin(2 * a);
        float vel = Mathf.Sqrt(Mathf.Abs(val));
        //if (val < 0)
        //    dir.y *= -1;
        return vel * dir.normalized;
    }


    public static Mesh ExtendSprite(Sprite spritePiece, float height)
    {
        if (!spritePiece)
        {
            Debug.LogWarning("There is no Sprite Piece to extend!");
            return null;
        }

        List<Vector3> v = new List<Vector3>();
        List<Vector2> u = new List<Vector2>();
        List<int> t = new List<int>();

        float widthDiv = 1f / spritePiece.texture.width;
        float heightDiv = 1f / spritePiece.texture.height;

        float x = spritePiece.rect.x;
        float y = spritePiece.rect.y;
        float w = spritePiece.rect.x + spritePiece.rect.width;
        float h = spritePiece.rect.y + spritePiece.rect.height;

        float curHeight = 0;
        float remainHeight = height;
        while (remainHeight > 0)
        {
            float chainHeight = Mathf.Min(remainHeight, spritePiece.rect.height);
            int vertexCount = v.Count;

            v.Add(new Vector3(-spritePiece.rect.width / 2f, curHeight + chainHeight, 0));
            v.Add(new Vector3(+spritePiece.rect.width / 2f, curHeight + chainHeight, 0));
            v.Add(new Vector3(+spritePiece.rect.width / 2f, curHeight, 0));
            v.Add(new Vector3(-spritePiece.rect.width / 2f, curHeight, 0));

            u.Add(new Vector2(x * widthDiv, (y + chainHeight) * heightDiv));
            u.Add(new Vector2(w * widthDiv, (y + chainHeight) * heightDiv));
            u.Add(new Vector2(w * widthDiv, y * heightDiv));
            u.Add(new Vector2(x * widthDiv, y * heightDiv));

            t.Add(vertexCount + 0);
            t.Add(vertexCount + 1);
            t.Add(vertexCount + 2);

            t.Add(vertexCount + 2);
            t.Add(vertexCount + 3);
            t.Add(vertexCount + 0);

            curHeight += chainHeight;
            remainHeight -= spritePiece.rect.height;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = v.ToArray();
        mesh.uv = u.ToArray();
        mesh.triangles = t.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }


}
