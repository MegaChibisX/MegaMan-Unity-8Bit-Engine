using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The parent script of all Vehicles.
/// To make a Ride, have your script inherit from this script and give it one GameObject that would act as a "Seat". This is where the player will be in when the ride moves.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Ride : MonoBehaviour
{


    public Transform seat;

    //[System.NonSerialized]
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
