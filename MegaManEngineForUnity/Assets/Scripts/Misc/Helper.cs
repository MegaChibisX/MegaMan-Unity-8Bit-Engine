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


}
