using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes the color of the sprute over time.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Sprite Color Over Time")]
public class Misc_SpriteColorOverTime : MonoBehaviour {

    public Gradient gradient;
    public AnimationCurve colorCurve;
    public SpriteRenderer rend;

    public float cycleLength = 1.0f;
    public bool unscaledTime = false;
    public bool loop = true;

    private float _time = 0.0f;
    private float cycleMult;

    private void Start()
    {
        if (rend == null)
        {
            rend = GetComponent<SpriteRenderer>();
            if (rend == null)
            {
                Debug.LogWarning("There is no Sprite Renderer assigned to " + name + "!");
                Destroy(this);
            }
        }

        cycleMult = 1f / cycleLength;
    }

    private void Update()
    {
        _time += cycleMult * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

        if (!loop && _time > cycleLength)
            Destroy(this);

        rend.color = gradient.Evaluate(colorCurve.Evaluate(_time % 1.0f));
    }


}
