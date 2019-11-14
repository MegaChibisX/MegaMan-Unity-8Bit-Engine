using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect for the final boss's background.
/// </summary>
[AddComponentMenu("MegaMan/Misc/Rotate Random")]
public class Misc_RotateRandom : MonoBehaviour
{

    public float speed = 10;
    public float strength = 36;
    public float curveStrength = 70;
    public Vector3 rotationAxis = Vector3.forward;
    public AnimationCurve curve;

    void Start()
    {
        rotationAxis = Random.rotationUniform * Vector3.forward;
    }
    void Update()
    {
        // Yeah, don't even bother. I just punched in random values until it looked nice.
        float c = curve.Evaluate(Time.time * curveStrength * Mathf.Deg2Rad);
        rotationAxis = Quaternion.Euler(new Vector3(Mathf.Sin(Time.time * strength * Mathf.Deg2Rad * c),
                                                    Mathf.Cos(Time.time * strength * Mathf.Deg2Rad * c),
                                                   -Mathf.Sin(Time.time * strength * Mathf.Deg2Rad * c) * 3) *
                                                   curve.Evaluate(Mathf.Abs(Mathf.Cos(Time.time * curveStrength * Mathf.Deg2Rad)))) * rotationAxis;
        transform.rotation = Quaternion.Euler(Vector3.Scale(rotationAxis, rotationAxis) * speed * Time.deltaTime) * transform.rotation;
    }

}
