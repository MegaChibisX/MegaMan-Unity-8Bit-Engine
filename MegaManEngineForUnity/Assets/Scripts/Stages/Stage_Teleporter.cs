using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_Teleporter : MonoBehaviour
{

    public Vector3 tpPos;
    public Collider2D trigger;

    public Vector2 leftCenter = Vector2.zero;
    public float maxRightMovement = 0.0f;
    public float maxUpMovement = 0.0f;

    public Boss boss;
    public bool activateBoss;
    public bool needBossDead;

    public Stage_Teleporter tpOther;
    public bool breakTp;

    public AudioClip newTrack;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player;
        if  (other.attachedRigidbody != null && (player = other.attachedRigidbody.GetComponent<Player>()) != null)
        {
            if ((boss && activateBoss) || (!boss && needBossDead) || (!activateBoss && !needBossDead))
            {
                player.SetWeapon(player.currentWeaponIndex, true);
                player.StopAllCoroutines();
                StartCoroutine(Teleport(player));
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Draws the teleport position.
        Gizmos.color = new Color(0.5f, 0, 0.7f);
        Gizmos.DrawSphere(tpPos, 4f);
        // Draws the camera  borders.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(leftCenter + Vector2.right * (maxRightMovement * 0.5f + 8.0f) + Vector2.up * (maxUpMovement * 0.5f + 8.0f), new Vector3(maxRightMovement + 272.0f, 240.0f + maxUpMovement, 0.0f));
    }


    public void Break()
    {
        if (trigger)
        {
            GetComponent<Animator>().Play("Broken");
            Destroy(trigger.gameObject);
        }
    }

    private IEnumerator Teleport(Player target)
    {
        target.SetGear(false, false);
        target.StopAllCoroutines();
        target.canMove = false;
        target.canBeHurt = false;
        target.body.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.25f);

        target.canAnimate = false;
        target.anim.Play("Outro");
        Helper.PlaySound(target.SFXLibrary.TpOut);

        yield return new WaitForSeconds(0.5f);

        CameraCtrl.instance.SetNewCameraBorders(leftCenter, maxRightMovement, maxUpMovement, false);
        CameraCtrl.instance.PlayMusic(newTrack);
        target.anim.Play("Stand");
        target.transform.position = tpPos;

        if (boss != null && activateBoss)
        {
            if (Player.instance != null)
                Player.instance.body.isKinematic = false;
            yield return new WaitForSeconds(1.0f);
            if (Player.instance != null)
                Player.instance.body.isKinematic = true;
            Player.instance.canBeHurt = true;
            boss.StartFight();
        }
        else
        {
            if (tpOther!= null && breakTp && boss == null)
            {
                tpOther.Break();
            }
        }

        target.canMove = true;
        target.canAnimate = true;
        target.canBeHurt = true;


    }


}
