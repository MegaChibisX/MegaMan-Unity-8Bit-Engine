using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    // Doesn't take drag into consideration, so it could stand to be improved.
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
        return vel * dir.normalized;
    }


    // Moves to another stage.
    public static void GoToStage(string stageName)
    {
        SceneManager.LoadScene(stageName);
    }


    // Changes the aspect ratio of the screen to the appropriate one for a MM game.
    public static void SetAspectRatio(Camera cmr)
    {

        // Sets the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time).
        float targetaspect = 254.0f / 224.0f;

        // Determines the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // Current viewport height should be scaled by this amount.
        float scaleheight = windowaspect / targetaspect;

        // Obtains camera component so it can modify its viewport.

        // If scaled height is less than current height, adds letterbox.
        if (scaleheight < 1.0f)
        {
            Rect rect = cmr.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            cmr.rect = rect;
        }
        else // Adds pillarbox.
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = cmr.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            cmr.rect = rect;
        }
    }


    // Extends a sprite to repeat until it reaches a specified height,
    // then returns a 3D mesh with it.
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
        //float h = spritePiece.rect.y + spritePiece.rect.height;

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
