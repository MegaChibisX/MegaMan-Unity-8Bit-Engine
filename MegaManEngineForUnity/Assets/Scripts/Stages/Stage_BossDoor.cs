using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the in-game object that handles camera transitions.
/// Can be used both with doors and without them.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Boss Door")]
public class Stage_BossDoor : MonoBehaviour {


    // 
    public int doorSize = 4;
    public SpriteRenderer[] doorTops;
    private SpriteRenderer[,] doorPieces;
    // The sound and component to play the sound.
    public AudioClip shutterSound;
    private AudioSource audSource;

    // The two sides back and forth that the camera switches to.
    public Stage_ChangeCameraBorders bordersRedSide;
    public Stage_ChangeCameraBorders bordersGreenSide;

    public Boss bossToActivate;
    public AudioClip newTrack;

    public int doorWidth = 2;       // Each 16 pixels count as 1 unit.

    private bool isActive = false;


    protected void Start()
    {
        audSource = GetComponentInChildren<AudioSource>();

        if (doorTops.Length > 0)
        {
            doorPieces = new SpriteRenderer[doorTops.Length, doorSize];
            for (int i = 0; i < doorTops.Length; i++)
            {
                doorPieces[i, 0] = doorTops[i];
                for (int j = 1; j < doorSize; j++)
                    doorPieces[i, j] = Instantiate(doorPieces[i, 0]);

                for (int j = 1; j < doorSize; j++)
                {
                    doorPieces[i, j].transform.parent = doorPieces[i, 0].transform;
                    doorPieces[i, j].transform.localPosition = new Vector3(0, -16 * j, 0);
                    doorPieces[i, j].transform.localRotation = Quaternion.identity;
                    doorPieces[i, j].transform.localScale = Vector3.one;
                }
            }
        }
    }
    public bool CheckForEmptySpace(Player contact)
    {
        float moveRightMult = Vector2.Angle(transform.right, contact.transform.position - transform.position) > 90 ? 1.0f : -1.0f;
        float moveDistance = contact.width * 2.0f + doorWidth * 16.0f;

        return Physics2D.OverlapBox(contact.transform.position + transform.right * moveDistance * moveRightMult,
                                    new Vector2(contact.width, contact.height),
                                    0, 1 << 8) == null;
    }
    /// <summary>
    /// Moves the player through the boss doors.
    /// </summary>
    /// <param name="contact">The player that made contact with the door.</param>
    /// <param name="doorWidth">The number of doors the player has to go through. Each 16 pixels count as 1 unit.</param>
    /// <returns></returns>
    public IEnumerator moveThroughDoors(Player contact)
    {
        if (isActive || GameManager.bossesActive > 0)
            yield break;

        float moveRightMult = Vector2.Angle(transform.right, contact.transform.position - transform.position) > 90 ? 1.0f : -1.0f;
        float moveDistance = contact.width * 2.0f + doorWidth * 16.0f;
        Time.timeScale = 0.0f;

        isActive = true;
        GameManager.roomFinishedLoading = false;
        contact.body.velocity = new Vector2(contact.body.velocity.x, -1);

        if (Player.instance != null)
        {
            Player.instance.CanMove(false);
            Player.instance.SetGear(false, false);
            Player.instance.canBeHurt = false;
        }

        float waitTime = 0.3f;
        while (waitTime > 0.0f)
        {
            waitTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        waitTime = 0.0f;
        for (int i = doorSize; i > 0; i--)
        {
            for (int j = 0; j < doorTops.Length; j++)
            {
                if (doorPieces[j, i - 1] != null)
                    doorPieces[j, i - 1].gameObject.SetActive(false);
            }

            audSource.PlaySound(shutterSound, true);
            yield return new WaitForSecondsRealtime(0.13f);
        }


        if (moveRightMult > 0)
        {
            if (bordersGreenSide == null)
                Debug.LogWarning("There is no Camera for the green side screen transition of " + gameObject.name);
            else
                CameraCtrl.instance.SetNewCameraBorders(bordersGreenSide.leftCenter, bordersGreenSide.maxRightMovement, bordersGreenSide.maxUpMovement);
        }
        else
        {
            if (bordersRedSide == null)
                Debug.LogWarning("There is no Camera for the red side screen transition of " + gameObject.name);
            else
                CameraCtrl.instance.SetNewCameraBorders(bordersRedSide.leftCenter, bordersRedSide.maxRightMovement, bordersRedSide.maxUpMovement);
        }

        while (moveDistance > 0.0f)
        {
            contact.transform.position += transform.right * 32.0f * Time.unscaledDeltaTime * moveRightMult;
            moveDistance -= 32.0f * Time.unscaledDeltaTime;
            yield return null;
        }

        waitTime = 0.0f;
        for (int i = 1; i <= doorSize; i++)
        {
            for (int j = 0; j < doorTops.Length; j++)
            {
                if (doorPieces[j, i - 1] != null)
                    doorPieces[j, i - 1].gameObject.SetActive(true);
            }

            audSource.PlaySound(shutterSound, true);
            yield return new WaitForSecondsRealtime(0.13f);
        }

        Time.timeScale = GameManager.globalTimeScale;
        GameManager.roomFinishedLoading = true;
        isActive = false;

        if (bossToActivate != null && bossToActivate.isActiveAndEnabled)
        {
            if (Player.instance != null)
                Player.instance.body.isKinematic = false;
            yield return new WaitForSeconds(1.0f);
            if (Player.instance != null)
                Player.instance.body.isKinematic = true;
            Player.instance.canBeHurt = true;
            bossToActivate.StartFight();
        }
        else if (Player.instance != null)
            {
                Player.instance.CanMove(true);
                Player.instance.canBeHurt = true;
            }

        if (newTrack)
            CameraCtrl.instance.PlayMusic(newTrack);

    }

    public IEnumerator moveThroughDoorlessRoom(Player contact)
    {
        if (isActive)
            yield break;

        float moveRightMult = Vector2.Angle(transform.right, contact.transform.position - transform.position) > 90 ? 1.0f : -1.0f;

        if ((moveRightMult > 0 && CameraCtrl.instance.leftCenter == bordersGreenSide.leftCenter
                && CameraCtrl.instance.maxRightMovement == bordersGreenSide.maxRightMovement && CameraCtrl.instance.maxUpMovement == bordersGreenSide.maxUpMovement) ||
            (moveRightMult < 0 && CameraCtrl.instance.leftCenter == bordersRedSide.leftCenter
                && CameraCtrl.instance.maxRightMovement == bordersRedSide.maxRightMovement && CameraCtrl.instance.maxUpMovement == bordersRedSide.maxUpMovement))
            yield break;

        float moveDistance = contact.width * 3.0f;
        Time.timeScale = 0.0f;

        isActive = true;
        GameManager.roomFinishedLoading = false;
        contact.body.velocity = new Vector2(contact.body.velocity.x, -1);

        if (Player.instance != null)
        {
            Player.instance.CanMove(false);
            Player.instance.SetGear(false, false);
            Player.instance.canBeHurt = false;
        }

        float waitTime = 0.5f;
        while (waitTime > 0.0f)
        {
            waitTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (moveRightMult > 0)
        {
            if (bordersGreenSide == null)
                Debug.LogWarning("There is no Camera for the green side screen transition of " + gameObject.name);
            else
                CameraCtrl.instance.SetNewCameraBorders(bordersGreenSide.leftCenter, bordersGreenSide.maxRightMovement, bordersGreenSide.maxUpMovement);
        }
        else
        {
            if (bordersRedSide == null)
                Debug.LogWarning("There is no Camera for the red side screen transition of " + gameObject.name);
            else
                CameraCtrl.instance.SetNewCameraBorders(bordersRedSide.leftCenter, bordersRedSide.maxRightMovement, bordersRedSide.maxUpMovement);
        }

        while (moveDistance > 0.0f)
        {
            contact.transform.position += transform.right * 32.0f * Time.unscaledDeltaTime * moveRightMult;
            moveDistance -= 32.0f * Time.unscaledDeltaTime;
            yield return null;
        }

        waitTime = 1.0f;
        while (waitTime > 0.0f)
        {
            waitTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = GameManager.globalTimeScale;
        GameManager.roomFinishedLoading = true;
        isActive = false;

        if (bossToActivate != null && bossToActivate.isActiveAndEnabled)
        {
            if (Player.instance != null)
                Player.instance.body.isKinematic = false;
            yield return new WaitForSeconds(1.0f);
            if (Player.instance != null)
                Player.instance.body.isKinematic = true;
            Player.instance.canBeHurt = true;
            bossToActivate.StartFight();
        }
        else if (Player.instance != null)
        {
            Player.instance.CanMove(true);
            Player.instance.canBeHurt = true;
        }

        if (newTrack)
            CameraCtrl.instance.PlayMusic(newTrack);

    }


    private void OnDrawGizmosSelected()
    {
        // Displays the two borders that can be transitioned from this object.
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - transform.right * 16, 8.0f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + transform.right * 16 * doorWidth, 8.0f);
        Gizmos.DrawLine(transform.position - transform.right * 16, transform.position + transform.right * 16 * doorWidth);

        if (bordersRedSide != null)
            bordersRedSide.OnDrawGizmosSelected();
        if (bordersGreenSide != null)
            bordersGreenSide.OnDrawGizmosSelected();
    }

}
