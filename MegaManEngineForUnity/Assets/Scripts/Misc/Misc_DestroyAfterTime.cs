using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Destroys a gameobject after some seconds, which can ingnore or consider timeScale.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Destroy After Time")]
public class Misc_DestroyAfterTime : MonoBehaviour {


    public float time = 1.0f;
    public bool unscaledTime = false;


    private void Update()
    {
        time -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (time <= 0.0f)
            Destroy(gameObject);
    }

}
