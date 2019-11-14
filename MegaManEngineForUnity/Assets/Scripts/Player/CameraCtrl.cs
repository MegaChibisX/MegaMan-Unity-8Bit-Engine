using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script for camera that follows the player around.
/// The Camera follows the player, stopping when it goes too far out of an area,
/// and has the ability to smoothly transition between rooms.
/// </summary>
public class CameraCtrl : MonoBehaviour {

    // The active Camera will need to be accessed from
    // many different classes fairly often, so keeping
    // a static reference to it is very useful.
    public static CameraCtrl instance;

    // The transform of the player the Camera follows.
    public Transform player;

    // Unity's Camera component which is what shows the stage.
    public Camera cmr;

    // The Audio Source component that plays the music.
    public AudioSource aud;
    // The Audio Source that plays the earthquake sound.
    public AudioSource audQuake;
    public AudioClip quakeClip;

    // The Stage_ChangeCameraBorders class is used to update
    // the borders of the Camera when the player enters a new room.
    // startBorders updates the borders at the beginning of the
    // scene, if there is no checkpoint active.
    public Stage_ChangeCameraBorders startBorders;

    // These three variables limit the Camera's position between specific borders.
    public Vector2 leftCenter = Vector2.zero;
    public float maxRightMovement = 0.0f;
    public float maxUpMovement = 0.0f;

    // Checks if the camera is moving between borders, to prevent
    // the camera from snapping to being inside the limited area.
    private bool inTransition = false;

    // Adds shake to the camera
    public float shakeTime;
    public float shakeStrength;
    public bool shakeDropPlayer;


    // Keeps track of the window's size.
    public int winX;
    public int winY;


    public void Start()
    {
        instance = this;
        // If there is a Start Border and no Checkpoint active,
        // this sets the borders to the Start Border.
        if (startBorders != null && !GameManager.checkpointActive)
        {
            SetNewCameraBorders(startBorders, false);

            GameManager.checkpointCamera_LeftCenter = leftCenter;
            GameManager.checkpointCamera_MaxRightMovement = maxRightMovement;
            GameManager.checkpointCamera_MaxUpMovement = maxUpMovement;
        }
        // If there is a Checkpoint active,
        // this sets the borders to the Checkpoint's.
        else if (GameManager.checkpointActive)
        {
            leftCenter = GameManager.checkpointCamera_LeftCenter;
            maxRightMovement = GameManager.checkpointCamera_MaxRightMovement;
            maxUpMovement = GameManager.checkpointCamera_MaxUpMovement;
        }
        // If no Start Borders or Checkpoint exist, these values remain
        // unchanged. It is suggested that this is only used for debug purposes.


        // Sets the position of the camera to be within
        // the bounds it should be when the stage starts.
        Vector3 startPos = GetPositionWithinCameraBounds(GameManager.playerPosition);
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);

        // Sets the proper aspect ratio.
        winX = Screen.width;
        winY = Screen.height;
        Helper.SetAspectRatio(GetComponent<Camera>());

        if (player == null)
        {
            Player tryPlayer = FindObjectOfType<Player>();
            if (tryPlayer != null)
                player = tryPlayer.transform;
        }
    }

    private void LateUpdate()
    {
        // Moves the camera.
        MoveCamera();

        // Resets the camera's aspect ratio.
        if (winX != Screen.width || winY != Screen.height)
        {
            winX = Screen.width;
            winY = Screen.height;
            Helper.SetAspectRatio(GetComponent<Camera>());
        }

        // Adds shake to the camera.
        if (shakeTime > 0.0f)
        {
            transform.position += Random.rotation * Vector3.right * Random.Range(0, shakeStrength);
            shakeTime -= Time.deltaTime;
            audQuake.PlaySound(quakeClip, false);
        }
    }

    private void MoveCamera()
    {
        // If there is no player available, the camera can't move after the player.
        //if (player == null)
        //{
        //    //Debug.LogWarning("There is no player assigned to the camera: " + gameObject.name);
        //    return;
        //}

        // Sets the target position to be within the bounds of the camera.
        Vector3 targetPos = GetPositionWithinCameraBounds(GameManager.playerPosition);
        targetPos.z = transform.position.z;

        // If the camera is in a transition, it should slowly move towards the desired position.
        // If not, it should snap to that position.
        if (inTransition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.unscaledDeltaTime * 128.0f);
            if (Mathf.Approximately((targetPos - transform.position).sqrMagnitude, 0))
            {
                inTransition = false;
            }
        }
        else
            transform.position = targetPos;
    }
    public Vector2 GetPositionWithinCameraBounds(Vector2 position)
    {
        // Limits the position to be within the desired borders.
        Vector3 targetPos = new Vector3(position.x, position.y, 0);
        targetPos.y = Mathf.Clamp(targetPos.y, leftCenter.y + 8.0f, leftCenter.y + maxUpMovement + 8.0f);
        targetPos.x = Mathf.Clamp(targetPos.x, leftCenter.x + 8.0f, leftCenter.x + maxRightMovement + 8.0f);
        return targetPos;
    }
    
    public void SetNewCameraBorders(Vector2 _leftCenter, float _maxRightMovement, float _maxUpMovement, bool setToTransition = true)
    {
        // Sets the new camera borders and 
        // sets the camera to act as transitioning.
        leftCenter = _leftCenter;
        maxRightMovement = _maxRightMovement;
        maxUpMovement = _maxUpMovement;
        inTransition = setToTransition;
    }
    public void SetNewCameraBorders(Stage_ChangeCameraBorders borders, bool setToTransition = true)
    {
        // Sets the new camera borders from a Stage_ChangeCameraBorders class.
        SetNewCameraBorders(borders.leftCenter, borders.maxRightMovement, borders.maxUpMovement, setToTransition);
    }

    public void Shake(float time, float strength, bool dropPlayer)
    {
        shakeTime = time;
        shakeStrength = strength;
        shakeDropPlayer = dropPlayer;
    }
    public  void PlayMusic(AudioClip track)
    {
        if (track != aud.clip)
            aud.PlaySound(track, true);
    }

    private void OnDrawGizmosSelected()
    {
        // Displays the area the camera will display
        // with the current borders.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(leftCenter + Vector2.right * (maxRightMovement * 0.5f + 8.0f) + Vector2.up * (maxUpMovement * 0.5f + 8.0f), new Vector3(maxRightMovement + 272.0f, 240.0f + maxUpMovement, 0.0f));
    }

}
