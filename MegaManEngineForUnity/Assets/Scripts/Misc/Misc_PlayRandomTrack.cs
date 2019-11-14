using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays random music.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Play Random Track")]
public class Misc_PlayRandomTrack : MonoBehaviour {

    public AudioClip[] clips;

    void Start()
    {
        if (clips.Length > 0)
            GetComponent<AudioSource>().PlaySound(clips[Random.Range(0, clips.Length)], true);

        Destroy(this);
    }

}
