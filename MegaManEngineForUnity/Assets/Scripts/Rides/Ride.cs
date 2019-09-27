using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ride : MonoBehaviour
{


    public Transform seat;

    [System.NonSerialized]
    public Player rider;
    private bool _canBeRidden = true;
    public bool canBeRidden
    {
        set { _canBeRidden = value; }
        get { return _canBeRidden && rider == null; }
    }




    public virtual void Start()
    {
        if (seat == null)
            Debug.LogError("The ride requires a seat!");
    }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void LateUpdate() { }

    public virtual void Mount() { }
    public virtual void Dismount()
    {
        rider = null;
    }


}
