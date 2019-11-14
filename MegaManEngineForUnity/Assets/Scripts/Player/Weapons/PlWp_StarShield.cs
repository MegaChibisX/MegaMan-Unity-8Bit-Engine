using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shield that goes around the player, which can be thrown forward. Powered version will split into 3 directions when thrown.
/// </summary>
public class PlWp_StarShield : Pl_Weapon
{

    public Transform spriteContainer;
    public Player owner;

    // Variables about the shield movement.
    public float rotateSpeed = 360f;
    public float throwSpeed = 100;
    public bool thrown = false;
    public bool splitOnThrow = false;
    public GameObject splitGameObject;



    public override void Update()
    {
        base.Update();

        // Rotates the shield.
        spriteContainer.eulerAngles = spriteContainer.eulerAngles + Vector3.forward * rotateSpeed * Time.unscaledDeltaTime;
        if (owner)
        {
            // Moves to the player's position if not thrown.
            transform.position = owner.transform.position;
        }
    }


    public override void Deflect()
    {
        Throw();
        base.Deflect();
    }
    public void Throw()
    {
        if (thrown)
            return;

       
        // Changes rocks to bullets if shot.
        if (splitOnThrow)
        {
            foreach (Transform tr in spriteContainer.GetComponentsInChildren<Transform>())
            {
                if (tr != spriteContainer.transform)
                {
                    GameObject shot = Instantiate(splitGameObject);
                    shot.transform.position = tr.position;
                    shot.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90f, Vector3.forward) * (tr.position - transform.position));
                }
            }
            Destroy(gameObject);
        }
        // Shoots entire shield forward.
        else
        {
            body.velocity = owner.right * throwSpeed;
            destroyOnAllEnemies = false;
            destroyOnBigEnemies = false;
            destroyOnWalls = false;
            gameObject.AddComponent<Misc_DestroyAfterTime>().time = 20f;
            owner = null;
        }
        thrown = true;
    }

}
