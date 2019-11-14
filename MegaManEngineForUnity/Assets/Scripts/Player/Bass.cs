using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MegaMan/Allies/Bass")]
public class Bass : Player
{

    public enum ShootDir { Up, UpRight, Right, DownRight }
    [System.NonSerialized]
    public ShootDir shootDir = ShootDir.Right;


    protected override void Animate()
    {
        // When the played animation is handled externally,
        // like during a combo or cutscene, default animations are ignored.
        if (!canAnimate)
            return;


        // Instead of using the same code multiple times for each attacking state,
        // animations just use a suffix in their Animator name.
        // Everything before the suffix should be the same for this to work correctly.
        string nameSuffix = "";

        if (shootTime > 0.0f)
        {
            nameSuffix = "Shoot";
            switch (shootDir)
            {
                case ShootDir.Up:
                    nameSuffix += "U";
                    break;
                case ShootDir.UpRight:
                    nameSuffix += "UR";
                    break;
                case ShootDir.DownRight:
                    nameSuffix += "DR";
                    break;
            }
        }
        else if (throwTime > 0.0f)
            nameSuffix = "Throw";



        // Simply checks the state of the player and players the appropriate animation.

        if (knockbackTime > 0.0f)
        {
            anim.Play("Hurt");
            return;
        }

        if (state == PlayerStates.Climb)
        {
            if (input.x != 0)
                lastLookingLeft = input.x < 0;
            if (shootTime > 0.0f || throwTime > 0.0f)
                anim.transform.localScale = new Vector3((lastLookingLeft ? -1 : 1), anim.transform.localScale.y, anim.transform.localScale.z);

            anim.Play("Climb" + nameSuffix);
            anim.speed = Mathf.Abs(input.y);
            return;
        }

        if (input.x != 0 && canMove && !paused)
        {
            anim.transform.localScale = new Vector3(input.x > 0 ? 1 : -1, anim.transform.localScale.y, anim.transform.localScale.z);
            lastLookingLeft = anim.transform.localScale.x < 0;
        }

        if (!isGrounded)
            anim.Play("Jump" + nameSuffix);
        else
        {
            if (slideTime > 0)
                anim.Play("Slide");
            else if (input.x != 0 && canMove && !paused)
                anim.Play("Run" + nameSuffix);
            else
                anim.Play("Stand" + nameSuffix);
        }
    }


}
