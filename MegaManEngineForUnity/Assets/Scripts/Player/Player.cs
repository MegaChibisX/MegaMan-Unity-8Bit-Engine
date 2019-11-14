using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("MegaMan/Allies/Player")]
public class Player : MonoBehaviour
{

    public static Player instance;

    // The player uses their own timeScale, mostly for the Speed Geear to work without slowing them down.
    public static float timeScale = 1.0f;
    public static float deltaTime
    {
        get
        {
            return Time.unscaledDeltaTime * timeScale;
        }
    }
    public static float fixedDeltaTime
    {
        get
        {
            return Time.fixedUnscaledDeltaTime * timeScale * (Time.timeScale == 0 ? 0 : (1 / Time.timeScale));
        }
    }
    
    // Basic player data.
    public Animator anim;
    public Rigidbody2D body;
    public bool isSpriteHidden = false;
    public SpriteRenderer sprite;
    public PlayerHealthbars bars;
    public GameManager.Players curPlayer;
    public Pl_WeaponData.WeaponColors defaultColors = new Pl_WeaponData.WeaponColors(
                                                  new Color(0f / 255f, 160f / 255f, 255f / 255f), new Color(0f / 256f, 97f / 256f, 255f / 255), Color.black);

    // Input.
    public Vector2 input;

    // Colliders.
    public Collider2D normalCol;
    public Collider2D slideCol;
    public GameObject spriteContainer;

    // Audio.
    public AudioSource audioWeapon;
    public AudioSource audioStage;

    // Color sprites.
    public SpriteRenderer bodyColorLight;
    public SpriteRenderer bodyColorDark;
    public SpriteRenderer bodyColorOutline;

    // Gear data.
    public GameObject speedGearTrail;
    public ParticleSystem gearSmoke;
    public GameObject gearBar;
    private Material gearBarMaterial;

    // Death explosion.
    public GameObject deathExplosion;

    // Audio clips.
    public PlayerSFX SFXLibrary;

    // Weapon data.
    public Pl_WeaponData currentWeapon;
    public Pl_WeaponData.Weapons defaultWeapon = Pl_WeaponData.Weapons.MegaBuster;
    public List<Pl_WeaponData.Weapons> weaponList;
    public int currentWeaponIndex = 0;

    // Pause menu.
    public Menu_Pause pauseMenu;
    public bool paused = false;

    // Cutscene items. Most unused.
    public bool useIntro = true;
    public Menu_Cutscene cutscene;
    [System.NonSerialized]
    public Vector2 prePauseVelocity;

    
    // States of the player.
    public enum PlayerStates { Normal, Still, Frozen, Climb, Hurt, Fallen, Paused, Riding }
    public PlayerStates state = PlayerStates.Normal;

    public bool canMove = true;
    public bool canBeHurt = true;
    public bool canAnimate = true;

    // Health data.
    public float health = 28;
    public float maxHealth = 28;
    protected float knockbackTime = 0.0f;
    protected float invisTime = 0.0f;

    // Is the double gear available? It can be disabled in the GameManager.
    public bool gearAvailable
    {
        get
        {
            return GameManager.item_DoubleGear;
        }
        set
        {
            GameManager.item_DoubleGear = value;
        }
    }
    public float gearGauge = 28;

    [System.NonSerialized]
    public bool gearActive_Speed = false;
    [System.NonSerialized]
    public bool gearActive_Power = false;

    // Gravity.
    public bool gravityInverted = false;
    public float gravityScale = 100.0f;
    public float gravityEnvironmentMulti = 1.0f;
    protected Vector2 windVector;

    // Velocities.
    public float moveSpeed = 80;
    public float climbSpeed = 80;
    public float jumpForce = 370;

    // Jumps.
    public int maxJumps = 1;
    protected int jumpsLeft = 0;

    // Slides.
    public enum SlideType { Slide, Dash, None }
    public SlideType slideType = SlideType.Slide;

    public float slideTime
    {
        get;
        protected set;
    }
    protected bool dashing = false;

    // Not used for charging anymore, but it has a minor role.
    protected float chargeKeyHold = 0f;
    [System.NonSerialized]
    public float shootTime = 0f;
    [System.NonSerialized]
    public float throwTime = 0f;

    // Spaces out trails.
    protected float gearTrailTime = 0.0f;
    protected bool gearRecovery = false;

    // The vehicle the player is riding. Engine comes with Rush Jet and Metal Wheel, but you can freely add anything like a Ride Armor if you want.
    public Ride ride;

    // Information about the collider.
    public float width = 14.0f;
    public float height = 17.67f;
    public Vector3 center = new Vector3(0.0f, -2.32f, 0.0f);
    public Vector3 _center
    {
        get
        {
            return new Vector3(center.x, center.y * (gravityInverted ? -1 : 1), center.z);
        }
    }
    [System.NonSerialized]
    public bool lastLookingLeft = false;

    public Vector3 right
    {
        get
        {
            return transform.right * (lastLookingLeft ? -1 : 1);
        }
    }
    public Vector3 up
    {
        get
        {
            if (anim == null)
                return Vector3.up;
            return transform.up * (anim.transform.localScale.y > 0 ? 1 : -1);
        }
    }


    protected virtual void Start()
    {
        // Keeps a reference for the player that other scripts can find quickly.
        instance = this;

        if (useIntro)
        {
            // If there is a checkpoint active, you should spawn at the checkpoint.
            if (GameManager.checkpointActive)
            {
                transform.position = GameManager.checkpointLocation;
                // Some global variables need to be reset every time the room is reset. Go to GameManager for more information.
                GameManager.ResetRoom();
            }
            // If there is no checkpoint active, teleport right where you are and save a checkpoint.
            else
            {
                if (CameraCtrl.instance == null)
                    FindObjectOfType<CameraCtrl>().Start();


                GameManager.checkpointLocation = transform.position;
                GameManager.checkpointActive = true;
                GameManager.checkpointCamera_LeftCenter = CameraCtrl.instance.leftCenter;
                GameManager.checkpointCamera_MaxRightMovement = CameraCtrl.instance.maxRightMovement;
                GameManager.checkpointCamera_MaxUpMovement = CameraCtrl.instance.maxUpMovement;
                // Some global variables need to be set once a stage starts. Go to GameManager for more information.
                GameManager.StartRoom();
            }
        }

        // Keeps track of some necessary components of the player.
        body = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        // Sets the weapons the player should have at the beginning of the stage.
        RefreshWeaponList();
        if (currentWeaponIndex >= weaponList.Count)
            currentWeaponIndex = weaponList.Count - 1;
        else if (currentWeaponIndex < 0)
            currentWeaponIndex = 0;

        // Variable resets.
        if (health == 0)
        { 
            health = maxHealth;
            SetWeapon(currentWeaponIndex);
        }
        slideCol.enabled = false;
        SetState(PlayerStates.Normal);

        // Sets the material for the Gear Bar. Look at the documentation for more information.
        if (gearBar != null)
            gearBarMaterial = gearBar.GetComponent<MeshRenderer>().material;

        // Plays the player intro, if this is the beginning of a room.
        if (useIntro)
            StartCoroutine(PlayIntro());
    }
    protected virtual void Update()
    {
        // If there is a cutscene that must be playing, play the cutscene.
        if (cutscene.isActive)
            cutscene.Update();

        // Updates global variables from the Game Manager.
        UpdateGameManager();

        // Mostly input and pause menu right now.
        HandleInput_Technical();

        if (canMove && !paused)
        {
            // Sets the jump counter.
            if ((isGrounded || isInSand) && jumpsLeft != maxJumps)
                jumpsLeft = maxJumps;
            else if (!isGrounded && jumpsLeft == maxJumps)
                jumpsLeft--;

            if (isGrounded && dashing && slideTime <= 0.0f && body.velocity.y * gravityScale >= 0)
                dashing = false;

            // The played can only move in some States.
            switch (state)
            {
                default:
                    HandleInput_Movement();
                    if (!wasGrounded && isGrounded)
                        Land();
                    break;
                case PlayerStates.Fallen:
                case PlayerStates.Climb:
                case PlayerStates.Hurt:
                case PlayerStates.Paused:
                    break;
                case PlayerStates.Riding:
                    if (ride == null)
                        Dismount();
                    else
                        Ride();
                    break;
            }
            switch (state)
            {
                case PlayerStates.Climb:
                case PlayerStates.Normal:
                    // Handles attack related activities.
                    HandleInput_Attacking();
                    break;
                default:
                    break;

            }
        }

        // Handles animation related activities.
        Animate();
        // Handles timer related activities.
        HandleInput_Timers();
        // wasGrounded is used to check if the player was on the ground in the previous frame.
        // After everything else has been done for the current frame, this variable is set to reflect the current frame,
        // as it will be red in the next one again.
        wasGrounded = isGrounded;
    }
    protected virtual void LateUpdate()
    {
        // Gets rid of the Player sprites if they should be invisible
        if (isSpriteHidden) 
        {
            if (spriteContainer.activeSelf)
                spriteContainer.SetActive(false);
        }
        else
        {
            if (!spriteContainer.activeSelf)
                spriteContainer.SetActive(true);
        }
        // The color of the player needs to change depending on the player's weapon
        // and their level of charge.

        Pl_WeaponData.WeaponColors colors = currentWeapon == null ? defaultColors : currentWeapon.GetColors();

        // bodyColorDark    = MegaMan's blue color
        // bodyColorLight   = MegaMan's cyan color
        // bodyColorOutline = MegaMan's line color

        // If the desired color's alpha value is 0, then the player's color
        // will be the default color. This happens on single color basis, so
        // the player can, for example, only have their outline glow.
        if (bodyColorDark != null)
        {
            bodyColorDark.color = colors.colorDark;
            if (bodyColorDark.color.a == 0)
                bodyColorDark.color = defaultColors.colorDark;
        }
        if (bodyColorLight != null)
        {
            bodyColorLight.color = colors.colorLight;
            if (bodyColorLight.color.a == 0)
                bodyColorLight.color = defaultColors.colorLight;
        }
        if (bodyColorOutline != null)
        {
            bodyColorOutline.color = colors.colorOutline;
            if (bodyColorOutline.color.a == 0)
                bodyColorOutline.color = defaultColors.colorOutline;
        }

        // Invisibility flashing
        if (spriteContainer != null && !isSpriteHidden)
        {
            if (invisTime > 0 && invisTime % 0.2f < 0.1f)
                spriteContainer.SetActive(false);
            else if (!spriteContainer.activeSelf)
                spriteContainer.SetActive(true);
        }
    
        ApplyGravity();
    }
    protected virtual void FixedUpdate()
    {
        // Moves with wind.
        if (canMove)
            body.position += (Vector2)windVector;
        windVector = Vector3.zero;

        // Handles movement related activities that should be done in FixedUpdate
        HandlePhysics_Movement();

        // Slow down the player based on how slow their local time is.
        // For example, this will be used when slowed down by their own Speed Gear.
        if (Time.timeScale == 0.0f)
            body.velocity = new Vector2(0, body.velocity.y);
        else
            body.velocity = new Vector2(body.velocity.x * timeScale / GameManager.globalTimeScale, body.velocity.y);
    }
    protected virtual void OnDrawGizmosSelected()
    {
        // This is all for better display of the character.
        // Comment out random lines and see what happens.
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + _center, new Vector3(width, height, width));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + _center, transform.position + _center - up * height);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + _center, transform.position + _center + right * width);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawCube(transform.position + _center - up * height * 0.3f, new Vector3(width, height * 0.4f, width));
        Gizmos.color = facesWall ? Color.green : Color.red;
        Gizmos.DrawCube(transform.position + _center + right * width * 0.4f, new Vector3(width * 0.1f, height, width));
        Gizmos.color = canStand ? Color.red : Color.green;
        Gizmos.DrawCube(transform.position + _center + up * height * 0.3f, new Vector3(width * 0.5f, height * 0.2f, width));
    }
    protected virtual void OnGUI()
    {
        // x and y are about the size of one in-game pixel.
        // cmrBase is the base of the playable viewport, right next
        // to the left black border.
        float x = Camera.main.pixelWidth / 256.0f;
        float y = Camera.main.pixelHeight / 218.0f;
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);

        // Reads and displays the healthbar.
        Sprite healthBar = bars.healthBar;
        Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
        Sprite emptyBar = bars.emptyBar;
        Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
        for (int i = 0; i < maxHealth; i++)
        {
            if (health > i)
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 24f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
            else
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 24f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
        }

        // Displays the UI for the current weapon.
        if (currentWeapon != null)
        {
            currentWeapon.OnGUI(x, y);
        }

        // If you have the Double Gear, display some of its UI.
        if (gearAvailable)
        {
            Sprite rectSprite = null;
            // Gear Background
             rectSprite= bars.gearBackground;
            Rect gearRect = new Rect(rectSprite.rect.x / rectSprite.texture.width, rectSprite.rect.y / rectSprite.texture.height,
                           rectSprite.rect.width / rectSprite.texture.width, rectSprite.rect.height / rectSprite.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (74), x * 16, y * 16), rectSprite.texture, gearRect);

            // Double Gear
            if (gearActive_Speed && gearActive_Power && (Time.unscaledTime % 0.25f) > 0.125f)
                rectSprite = bars.doubleGear2;
            else
                rectSprite = bars.doubleGear1;
            gearRect = new Rect(rectSprite.rect.x / rectSprite.texture.width, rectSprite.rect.y / rectSprite.texture.height,
                           rectSprite.rect.width / rectSprite.texture.width, rectSprite.rect.height / rectSprite.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (74), x * 16, y * 16), rectSprite.texture, gearRect);

            // Speed Gear
            if (gearActive_Speed && (Time.unscaledTime % 0.25f) > 0.125f)
                rectSprite = bars.speedGear2;
            else
                rectSprite = bars.speedGear1;
            gearRect = new Rect(rectSprite.rect.x / rectSprite.texture.width, rectSprite.rect.y / rectSprite.texture.height,
                           rectSprite.rect.width / rectSprite.texture.width, rectSprite.rect.height / rectSprite.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (74), x * 16, y * 16), rectSprite.texture, gearRect);

            // Power Gear
            if (gearActive_Power && (Time.unscaledTime % 0.25f) > 0.125f)
                rectSprite = bars.powerGear2;
            else
                rectSprite = bars.powerGear1;
            gearRect = new Rect(rectSprite.rect.x / rectSprite.texture.width, rectSprite.rect.y / rectSprite.texture.height,
                           rectSprite.rect.width / rectSprite.texture.width, rectSprite.rect.height / rectSprite.texture.height);
            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (74), x * 16, y * 16), rectSprite.texture, gearRect);
        }

        // Display the pause menu on top of everything.
        if (paused && pauseMenu != null)
        {
            pauseMenu.DrawGUI();
        }

        // Display the active cutscene on further top of everything.
        if (cutscene.isActive)
            cutscene.DrawGUI();
    }
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D otherBody = collision.collider.attachedRigidbody;
        if (otherBody != null)
        {
           // If in contact with an enemy, take damage.
            if (otherBody.GetComponent<Enemy>())
            {
                if (invisTime <= 0.0f && canBeHurt && collision.gameObject.tag != "IgnorePlayer")
                    Damage(otherBody.GetComponent<Enemy>().damage);
            }
        }
    }
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        Rigidbody2D otherBody = collision.collider.attachedRigidbody;
        if (otherBody != null)
        {
            if (otherBody.gameObject.layer == 13)
            {
                print(collision.contacts[0].point);
                print("S");
                Kill();
            }
            // If in contact with a Boss Door, go through the door.
            else if (otherBody.GetComponent<Stage_BossDoor>() && canMove)
            {
                Stage_BossDoor door = otherBody.GetComponent<Stage_BossDoor>();

                if (door.CheckForEmptySpace(this))
                {
                    SetGear(false, false);
                    StartCoroutine(door.moveThroughDoors(this));
                }
            }
        }
    }
    protected virtual void OnCollisionExit2D(Collision2D collision) { }
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        Rigidbody2D otherBody = collider.attachedRigidbody;
        if (otherBody != null)
        {
            // If in contact with enemy, take damage.
            if (otherBody.GetComponent<Enemy>() && collider.gameObject.tag != "IgnorePlayer")
            {
                if (invisTime <= 0.0f && canBeHurt)
                    Damage(otherBody.GetComponent<Enemy>().damage);

                if (otherBody.GetComponent<Bo_Roll3D>() && collider.name != "ColliderDamage")
                    otherBody.GetComponent<Bo_Roll3D>().hitPlayer = true;
            }
            // If in contact with a Boss Door, go through it.
            else if (otherBody.GetComponent<Stage_BossDoor>())
            {
                StartCoroutine(otherBody.GetComponent<Stage_BossDoor>().moveThroughDoorlessRoom(this));
            }
            // If in contact with a Gravity Switch, switch the gravity appropriately.
            else if (otherBody.GetComponent<Stage_GravitySwitch>())
            {
                gravityInverted = !otherBody.GetComponent<Stage_GravitySwitch>().gravityDown;
                SetGravity(gravityScale == 0 ? 0 : 1, gravityInverted);
            }
            // If in contact with a Checkpoint, register it in the Game Manager.
            else if (otherBody.GetComponent<Stage_Checkpoint>())
            {
                GameManager.checkpointLocation = otherBody.transform.position;
                GameManager.checkpointActive = true;
                GameManager.checkpointCamera_LeftCenter = CameraCtrl.instance.leftCenter;
                GameManager.checkpointCamera_MaxRightMovement = CameraCtrl.instance.maxRightMovement;
                GameManager.checkpointCamera_MaxUpMovement = CameraCtrl.instance.maxUpMovement;
            }
            else if (otherBody.GetComponent<Ride>())
            {
                Mount(otherBody.GetComponent<Ride>());
            }
        }
    }
    protected virtual void OnTriggerStay2D(Collider2D collider)
    {
        Rigidbody2D otherBody = collider.attachedRigidbody;
        if (otherBody != null)
        {
            if (otherBody.GetComponent<Stage_WindZone>() != null)
            {
                Stage_WindZone zone = otherBody.GetComponent<Stage_WindZone>();
                windVector += zone.direction.normalized * zone.strength * Time.fixedDeltaTime;
            }
            else if (otherBody.GetComponent<Stage_GravityScale>() != null)
            {
                gravityEnvironmentMulti *= otherBody.GetComponent<Stage_GravityScale>().gravityScale;
            }
            else  if (otherBody.GetComponent<Stage_Sand>() != null)
            {
                float sinkSpeed = otherBody.GetComponent<Stage_Sand>().sinkSpeed;

                if (body.velocity.y * anim.transform.localScale.y <= 0)
                    body.velocity = new Vector2(body.velocity.x, Mathf.Clamp(body.velocity.y, -sinkSpeed, sinkSpeed));
            }
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        Rigidbody2D otherBody = collider.attachedRigidbody;
        if (otherBody != null)
        {

        }
    }

    protected virtual void HandleInput_Movement()
    {
        // Jump button related actions.
        if (Input.GetButtonDown("Jump"))
        {
            // If the down button is pressed and you're on the ground, slide.
            if (input.y == -1 && isGrounded && slideType != SlideType.None)
            {
                if (slideTime <= 0.0f)
                {
                    slideTime = 0.5f;
                    if (slideType == SlideType.Slide)
                    {
                        slideCol.enabled = true;
                        normalCol.enabled = false;
                    }
                    else if (slideType == SlideType.Dash)
                        dashing = true;
                }
            }
            // If not in a slide, or in a slide without a block over the player, jump.
            else if (canStand &&
                        (jumpsLeft > 0 ||
                        isInSand))
            {
                body.velocity = new Vector2(body.velocity.x, jumpForce * timeScale / (GameManager.globalTimeScale != 0 ? GameManager.globalTimeScale : 1) * up.y);
                jumpsLeft--;
            }
        }
        // If the jump button is released mid-air, cut the player's jump short.
        else if (Input.GetButtonUp("Jump"))
        {
            if (body.velocity.y * up.y > 0)
                body.velocity = new Vector2(body.velocity.x, body.velocity.y * 0.5f);
        }
        // Slide
        if (Input.GetButtonDown("Slide") && slideType != SlideType.None)
        {
            slideTime = 0.5f;
            if (slideType == SlideType.Slide)
            {
                slideCol.enabled = true;
                normalCol.enabled = false;
            }
            else if (slideType == SlideType.Dash)
                dashing = true;
        }

        if (state != PlayerStates.Climb)
        {
            // Ladder layer = 10
            // Look for a ladder where the player is standing. If it's there, climb.
            if (Mathf.Abs(input.y) > 0.5f &&
                    Physics2D.OverlapPoint(transform.position + _center, 1 << 10))
            {
                transform.position = new Vector3(Mathf.Round((transform.position.x - 8.0f) / 16.0f) * 16.0f + 8.0f,
                    transform.position.y, transform.position.z);
                SetState(PlayerStates.Climb);
            }
            // Ladder layer = 10
            // Look for a ladder where under the player. If it's there, climb.
            else if (input.y < -0.5f &&
                        Physics2D.OverlapPoint(transform.position + _center - up * height * 0.6f, 1 << 10))
            {
                transform.position = new Vector3(Mathf.Round((transform.position.x - 8.0f) / 16.0f) * 16.0f + 8.0f,
                    transform.position.y - height * 0.6f, transform.position.z);
                SetState(PlayerStates.Climb);
            }
        }
    }
    protected virtual void HandleInput_Attacking()
    {
        // If there is an available weapon to use, you can use it.
        if (currentWeapon != null)
        {
            // Fire button events.
            if (Input.GetButtonDown("Fire1"))
                currentWeapon.Press();
            if (Input.GetButton("Fire1"))
            {
                currentWeapon.Hold();
                chargeKeyHold += Time.unscaledDeltaTime;
            }
            else
            {
                if (chargeKeyHold > 0.0f)
                    currentWeapon.Release();
                chargeKeyHold = 0;
            }
            // Some weapons need to do something in every Update.
            currentWeapon.Update();

            // Switch weapons.
            if (Input.GetButtonDown("WeaponSwitch"))
            {
                if (Input.GetAxis("WeaponSwitch") == 0)
                {
                    SetWeapon(0);
                }
                else
                {
                    currentWeaponIndex += Mathf.RoundToInt(Input.GetAxisRaw("WeaponSwitch"));

                    if (currentWeaponIndex < 0)
                        currentWeaponIndex = (int)weaponList.Count - 1;
                    else if (currentWeaponIndex >= (int)weaponList.Count)
                        currentWeaponIndex = 0;

                    if (currentWeapon != null)
                        currentWeapon.Cancel();
                    SetWeapon(currentWeaponIndex);
                }
            }
        }


        // If the gears can be used and they are not burnt out,
        // handle Double Gear. Still needs to be improved a bit.
        if (!gearRecovery && gearAvailable)
        {
            // If Power Gear key is held, then it goes Double Gear. If not, Speed Gear.
            if (Input.GetButtonDown("GearSwitch") && Input.GetAxisRaw("GearSwitch") <= 0)
            {
                if (Input.GetAxisRaw("GearSwitch") == 0 && !gearActive_Speed)
                    SetGear(true, true);
                else
                    SetGear(!gearActive_Speed, false);
            }
            // If Speed Gear key is held, then it goes Double Gear. If not, Power Gear.
            if (Input.GetButtonDown("GearSwitch") && Input.GetAxisRaw("GearSwitch") >= 0)
            {
                if (Input.GetAxisRaw("GearSwitch") == 0 && !gearActive_Power)
                    SetGear(true, true);
                else
                    SetGear(false, !gearActive_Power);
            }
        }

    }
    protected virtual void HandleInput_Technical()
    {
        // Registers the input in a local variable.
        input.y = Mathf.Clamp(Mathf.Round(Input.GetAxisRaw("Vertical") * 1.2f) * (gravityInverted ? -1 : 1), -1f, 1f);

        if (Input.GetAxisRaw("Horizontal") != 0)
            input.x = Mathf.Clamp(Mathf.Round(Input.GetAxisRaw("Horizontal") * 1.5f), -1f, 1f);
        else
        {
            if (Physics2D.Raycast(transform.position + _center, -up, height * 0.6f, 1 << 11))
            {
                input.x = Mathf.MoveTowards(input.x, Input.GetAxisRaw("Horizontal"), Time.unscaledDeltaTime * 0.5f);
            }
            else
                input.x = 0f;
        }

        // If the game is paused, shows the pause menu.
        if (paused)
        {
            // If there is no pause menu to show, resumes the game.
            if (pauseMenu == null)
            {
                Time.timeScale = GameManager.globalTimeScale;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
                SetLocalTimeScale(timeScale);
                paused = false;
            }
            else
                pauseMenu.Update();
        }
        else
        {
            // If the pause menu button is pressed, does everything that needs to be done for the game to be paused.
            if (Input.GetButtonDown("Start") && canMove)
            {
                GameManager.globalTimeScale = Time.timeScale;
                Time.timeScale = 0.0f;
                paused = true;

                pauseMenu = new Menu_Pause();
                pauseMenu.Start(this);

                prePauseVelocity = body.velocity;
                body.velocity = Vector2.zero;
                body.bodyType = RigidbodyType2D.Kinematic;
                body.simulated = false;
            }
        }
    }
    protected virtual void HandleInput_Timers()
    {
        // Time remaining for the player's slide.
        if (slideTime > 0)
        {
            if (!isGrounded)
                slideTime = 0.0f;

            slideTime -= deltaTime;

            if (!canStand)
                slideTime = 0.1f;

            if (slideTime <= 0)
            {
                slideCol.enabled = false;
                normalCol.enabled = true;
            }
        }


        // Time the player should stay in their shooting animation.
        if (shootTime > 0)
            shootTime -= deltaTime;
        // Time the player should stay in their throwing animation.
        if (throwTime > 0)
            throwTime -= deltaTime;


        if (!paused)
        {
            // Invisibility time, when the player can't get damaged.
            if (invisTime > 0.0f)
                invisTime -= deltaTime;
            // Knock back time, when the player will be frozen and moving back, usually after a hit.
            if (knockbackTime > 0.0f)
            {
                knockbackTime -= deltaTime;
                if (knockbackTime <= 0.0f)
                    SetState(PlayerStates.Normal);
            }
        }

        // How often a trail will spawn when the Speed Gear is active.
        // If the timer is <= 0, a clone will spawn and the timer will be reset.
        // This runs all the time, but the trail only spawns if the Speed Gear is active.
        gearTrailTime -= Time.unscaledDeltaTime;
        if (gearTrailTime <= 0.0f)
        {
            gearTrailTime = 0.2f;
            if (gearActive_Speed)
                MakeTrail();
            else if (dashing)
            {
                MakeTrail();
                gearTrailTime = 0.1f;
            }
        }

        // If either Gear is active, handles gear related activities.
        if (gearActive_Speed || gearActive_Power)
        {

            gearGauge -= Time.unscaledDeltaTime * 2.8f;
            // The bar over the player uses a Cutout shader for the circular gauge bar.
            // The Shader can be found in Assets/Sprites/GUI/CutoutSpriteShader.shader,
            // which is a slight edit on a default Unity shader.
            if (gearBarMaterial != null)
                gearBarMaterial.SetFloat("_CutoffStrength", 1.0f - gearGauge / 28.0f);

            // Burns out if out of Gear juice.
            if (gearGauge <= 0.0f)
            {
                SetGear(false, false);
                gearRecovery = true;
                if (gearGauge < 0)
                    gearGauge = 0;
                gearSmoke.Play();
            }
        }
        else
        {
            // If the gears were burnt out, the player recovers slowly. If not, recovers normally.
            if (gearRecovery)
                gearGauge += Time.unscaledDeltaTime * 2.8f;
            else
                gearGauge += Time.unscaledDeltaTime * 5.6f;

            // Shader related. Just don't worry about it.
            if (gearBarMaterial != null)
                gearBarMaterial.SetFloat("_CutoffStrength", 1.0f - gearGauge / 28.0f);

            // Just recovered!
            if (gearGauge >= 28.0f)
            {
                gearGauge = 28.0f;
                gearRecovery = false;
                gearSmoke.Stop();
            }
        }
        // (De)activates the UI gear bar.
        if (gearAvailable && gearGauge < 28.0f)
        {
            if (!gearBar.activeSelf)
                gearBar.SetActive(true);
        }
        else
        {
            if (gearBar.activeSelf)
                gearBar.SetActive(false);
        }

        // SetGravity(float magnitude, bool inverted) finds the appropriate gravity value when called,
        // so it just needs to be called when the gravity might need to be changed.
        if (isInWater != wasInWater)
        {
            SetGravity(gravityScale == 0 ? 0 : 1, gravityInverted);
            wasInWater = isInWater;
        }
        if (isInSand != wasInSand)
        {
            SetGravity(1, gravityInverted);
            wasInSand = isInSand;
        }
    }
    protected virtual void HandlePhysics_Movement()
    {
        if (canMove && !paused)
        {
            if (isGrounded && CameraCtrl.instance.shakeTime > 0 && CameraCtrl.instance.shakeDropPlayer)
                Earthquake(1.0f);

            // Decides how the player's should move.
            if (state == PlayerStates.Hurt)
                KnockBack();
            else if (state == PlayerStates.Climb)
                Climb();
            else if (state == PlayerStates.Riding)
            {
                if (ride != null)
                    transform.position = ride.seat.position;
            }
            else if (slideTime > 0)
                body.velocity = new Vector2(moveSpeed * 2f * anim.transform.localScale.x, body.velocity.y);
            else if (dashing)
                body.velocity = new Vector2(input.x * moveSpeed * 2f, body.velocity.y);
            else
                body.velocity = new Vector2(input.x * moveSpeed, body.velocity.y);

            // Conveyor movement.
            Collider2D col = null;
            if (col = Physics2D.OverlapBox(transform.position + _center - up * height * 0.5f,
                                            new Vector2(width * 0.95f, height * 0.1f), 0, 1 << 8))
            {
                if (col.gameObject.tag == "ConveyorLeft")
                    body.velocity += Vector2.left * 32f;
                else if (col.gameObject.tag == "ConveyorRight")
                    body.velocity += Vector2.right * 32f;
            }

            // Limits drop velocity
            float lmt = -400 / Time.timeScale * timeScale;
            if (body.velocity.y < lmt)
                body.velocity = new Vector2(body.velocity.x, lmt);
        }
    }
    protected virtual void Climb()
    {
        // If the player is not attacking, they can move up and down the ladder.
        body.velocity = Vector2.zero;
        if (shootTime <= 0.0f && throwTime <= 0.0f)
            body.MovePosition(transform.position + up * input.y * climbSpeed * fixedDeltaTime * Time.timeScale);

        if (!Physics2D.OverlapPoint(transform.position + _center - up * height * 0.5f, 1 << 10) ||
            Input.GetButton("Jump"))
        {
            SetState(PlayerStates.Normal);
        }
    }
    protected virtual void Animate()
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
            nameSuffix = "Shoot";
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
    protected void KnockBack()
    {
        // If not sliding, moves back. If sliding, stays in place.
        body.velocity = Vector3.zero;
        if (slideTime <= 0.0f && !paused)
            body.MovePosition(transform.position - right * 40 * Time.fixedDeltaTime);
    }
    protected virtual void Land()
    {
        // Plays the landing sound.
        audioStage.PlaySound(SFXLibrary.land, true);
        dashing = false;
    }
    protected  void Ride()
    {
        if (Input.GetButtonDown("Jump") && input.y > 0)
            Dismount();

        if (ride)
            transform.position = ride.seat.position;
    }

    public virtual void Damage(float damage)
    {
        // Takes health off, plays sounds, kills if needed,
        // sets the state to hurt, sets time for knockback and invisibility.
        if (damage <= 0)
            return;

        health -= damage;
        audioStage.PlaySound(SFXLibrary.hurt, true);
        if (health <= 0)
        {
            print("D");
            Kill();
        }

        SetState(PlayerStates.Hurt);
        knockbackTime = 0.3f;
        invisTime = 1.3f;
    }
    public virtual void Kill()
    {
        // Cancels active weapon and Gears.
        if (currentWeapon != null)
            currentWeapon.Cancel();

        SetGear(false, false);

        // Creates a Death Explosion object and gives it a Scene Reset Timer.
        GameObject o = Instantiate(deathExplosion);
        o.transform.position = transform.position;

        o.AddComponent<Misc_ResetScene>().timeToReset = 5.0f;
        CameraCtrl.instance.PlayMusic(null);

        Destroy(gameObject);
    }
    public void SetWeapon(int weaponIndex, bool force = false)
    {


        // If there are no available weapons, exit.
        if (weaponList == null || weaponList.Count == 0)
        {
            currentWeapon = null;
            return;
        }

        // Prevents out-of-bounds issues.
        if (weaponIndex >= weaponList.Count)
            weaponIndex = weaponList.Count - 1;
        else if (weaponIndex < 0)
            weaponIndex = 0;

        if (weaponIndex != currentWeaponIndex || force)
        {
            // Some weapons need to be cancelled when switched out.
            if (currentWeapon != null)
                currentWeapon.Cancel();
        }

        // Sets and starts the active weapon.
        currentWeaponIndex = weaponIndex;

        currentWeapon = Pl_WeaponData.WeaponList[(int)weaponList[weaponIndex]];
        currentWeapon.owner = this;

        currentWeapon.Start();
    }
    public void RefreshWeaponList()
    {
        // Sets up the weapons the player can use in game at this point.
        weaponList = new List<Pl_WeaponData.Weapons>();
        weaponList.Add(defaultWeapon);
        //weaponList.Add(Pl_WeaponData.Weapons.HyperBomb);
        //weaponList.Add(Pl_WeaponData.Weapons.MetalBlade);
        //weaponList.Add(Pl_WeaponData.Weapons.GeminiLaser);
        //weaponList.Add(Pl_WeaponData.Weapons.PharaohShot);
        //weaponList.Add(Pl_WeaponData.Weapons.StarCrash);
        //weaponList.Add(Pl_WeaponData.Weapons.WindStorm);
        //weaponList.Add(Pl_WeaponData.Weapons.BlackHoleBomb);
        //weaponList.Add(Pl_WeaponData.Weapons.CommandoBomb);

        if (GameManager.bossDead_BombMan)
            weaponList.Add(Pl_WeaponData.Weapons.HyperBomb);
        if (GameManager.bossDead_MetalMan)
            weaponList.Add(Pl_WeaponData.Weapons.MetalBlade);
        if (GameManager.bossDead_GeminiMan)
            weaponList.Add(Pl_WeaponData.Weapons.GeminiLaser);
        if (GameManager.bossDead_PharaohMan)
            weaponList.Add(Pl_WeaponData.Weapons.PharaohShot);
        if (GameManager.bossDead_StarMan)
            weaponList.Add(Pl_WeaponData.Weapons.StarCrash);
        if (GameManager.bossDead_WindMan)
            weaponList.Add(Pl_WeaponData.Weapons.WindStorm);
        if (GameManager.bossDead_GalaxyMan)
            weaponList.Add(Pl_WeaponData.Weapons.BlackHoleBomb);
        if (GameManager.bossDead_CommandoMan)
            weaponList.Add(Pl_WeaponData.Weapons.CommandoBomb);


        weaponList.Add(Pl_WeaponData.Weapons.RushCoil);
        weaponList.Add(Pl_WeaponData.Weapons.RushJet);
    }

    public void SetGear(bool speedGear, bool powerGear)
    {
        // If the Double Gear is active, switching Gears isn't available.
        if (gearActive_Speed && gearActive_Power && (!speedGear ^ !powerGear))
            return;

        gearActive_Speed = speedGear;
        gearActive_Power = powerGear;

        // Sets the timeScale, both globally and for the player.
        if (speedGear)
        {
            Time.timeScale = 0.25f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            GameManager.globalTimeScale = Time.timeScale;
            SetGravity(1.0f, gravityInverted);
            SetLocalTimeScale(1.0f);
        }
        else
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            GameManager.globalTimeScale = Time.timeScale;
            SetGravity(1.0f, gravityInverted);
            SetLocalTimeScale(1.0f);
        }

        // Cancels the current weapon, as things like charged shots can't carry over to the power gear.
        if (powerGear)
        {
            if (currentWeapon != null)
                currentWeapon.Cancel();
        }
        else
        {

        }
    }
    protected void MakeTrail()
    {

        if (speedGearTrail != null)
        {
            // Matches the current sprite with a black sprite and spawns it.
            Sprite blankSprite = null;
            string blankSpritePath = "Sprites/Players/MegaMan/MegaMan_Blank";

            switch (curPlayer)
            {
                case GameManager.Players.MegaManJet:
                    blankSpritePath = "Sprites/Players/MegaMan_Jet/MegaMan_Blank";
                    break;
                case GameManager.Players.ProtoMan:
                    blankSpritePath = "Sprites/Players/ProtoMan/ProtoMan_Blank";
                    break;
                case GameManager.Players.MegaManPower:
                    blankSpritePath = "Sprites/Players/MegaMan_Power/Power_Blank";
                    break;
                case GameManager.Players.Bass:
                    blankSpritePath = "Sprites/Players/Bass/Bass_Blank";
                    break;
                case GameManager.Players.X:
                    blankSpritePath = "Sprites/Players/X/X_Blank";
                    break;
            }

            Sprite[] subSprites = Resources.LoadAll<Sprite>(blankSpritePath);
            Sprite newSprite = Array.Find(subSprites, item => item.name == sprite.sprite.name);
            if (newSprite)
                blankSprite = newSprite;

            // This could be improved with pooling. If I forget to do it in the future and you can do it, feel free to.
            GameObject tr = Instantiate(speedGearTrail);
            tr.transform.position = transform.position;
            tr.transform.rotation = transform.rotation;
            tr.transform.localScale = anim.transform.localScale;

            tr.GetComponent<SpriteRenderer>().sprite = blankSprite;
            tr.GetComponent<SpriteRenderer>().flipX = sprite.flipX;
            tr.GetComponent<SpriteRenderer>().flipY = sprite.flipY;
        }


    }

    public virtual void Mount(Ride newRide)
    {
        // If there is no available ride, doesn't mount.
        if (!canMove || newRide == null || !newRide.canBeRidden || ride != null || newRide.transform.localScale.y != anim.transform.localScale.y)
            return;

        // Dismounts already existing ride.
        if (ride != null)
            Dismount();

        // Freezes the player.
        SetState(PlayerStates.Riding);
        transform.position = newRide.seat.position;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.velocity = Vector3.zero;

        ride = newRide;
        ride.rider = this;
        ride.Mount();
    }
    public virtual void Dismount()
    {
        // Allows the player to move again.
        body.bodyType = RigidbodyType2D.Dynamic;
        if (state == PlayerStates.Riding)
        {
            SetState(PlayerStates.Normal);
            // Jumps
            body.velocity = new Vector2(body.velocity.x, jumpForce * timeScale / GameManager.globalTimeScale * up.y);
        }

        // Dismounts from the ride.
        if (ride != null)
            ride.Dismount();
        ride = null;
    }
    public virtual void SetGrabbed(bool isGrabbed, bool hideSprite)
    {
        SetState(isGrabbed ? PlayerStates.Frozen : PlayerStates.Normal);
        SetGravity(isGrabbed ? 0 : 1, gravityInverted);
        isSpriteHidden = hideSprite;
    }


    public void SetState(PlayerStates newState)
    {
        // Each state has its own conditions that need to be met
        // for it to work as expected. These are handled here.
        state = newState;
        switch (state)
        {
            case PlayerStates.Normal:
                canMove = true;
                SetGravity(1.0f, gravityInverted);
                SetLocalTimeScale(timeScale);
                anim.speed = 1.0f;
                Dismount();
                break;
            case PlayerStates.Climb:
                canMove = true;
                body.velocity = Vector3.zero;
                SetGravity(0.0f, gravityInverted);
                anim.speed = 0.0f;
                Dismount();
                break;
            case PlayerStates.Still:
                canMove = false;
                body.velocity = Vector3.zero;
                SetGravity(1.0f, gravityInverted);
                SetLocalTimeScale(timeScale);
                anim.speed = 1.0f;
                Dismount();
                break;
            case PlayerStates.Hurt:
                canMove = true;
                SetGravity(1.0f, gravityInverted);
                anim.speed = 1.0f;
                break;
            case PlayerStates.Frozen:
                canMove = false;
                body.velocity = Vector3.zero;
                SetGravity(0.0f, gravityInverted);
                anim.speed = 1.0f;
                break;
            case PlayerStates.Fallen:
                canMove = false;
                SetGravity(1.0f, gravityInverted);
                SetLocalTimeScale(timeScale);
                anim.speed = 1.0f;
                break;
            case PlayerStates.Paused:
                canMove = false;
                SetGravity(0.0f, gravityInverted);
                SetLocalTimeScale(timeScale);
                anim.speed = 1.0f;
                break;
        }
    }
    public void CanMove(bool _canMove)
    {
        // Freezes the player.
        canMove = _canMove;
        body.isKinematic = !_canMove;
        if (!_canMove)
        {
            body.velocity = Vector2.zero;
        }
    }
    public void SetLocalTimeScale(float _timeScale)
    {
        // Based on this: https://answers.unity.com/questions/711749/slow-time-for-a-single-rigid-body.html

        float prevMass = body.mass;
        body.mass = 1.0f;

        _timeScale = Mathf.Abs(_timeScale);
        timeScale = _timeScale;

        // This works, don't question it.
        body.mass /= _timeScale;
        body.mass *= Time.timeScale;
        body.velocity *= prevMass / body.mass;
    }
    public void ApplyGravity()
    {
        if (!body.isKinematic && !paused)
            body.velocity += Vector2.down * 10f * gravityScale * gravityEnvironmentMulti * body.mass * body.mass * fixedDeltaTime;
        gravityEnvironmentMulti = 1.0f;
    }
    public virtual void SetGravity(float magnitude, bool inverted)
    {
        // Gets out of the ride if the gravity switches
        if (inverted != gravityInverted)
            Dismount();

        // The collider of the player needs to be considered when the player flips,
        // not the center of the player themselves.
        if (inverted)
            anim.transform.localPosition = center * 2.0f;
        else
            anim.transform.localPosition = Vector3.zero;
        anim.transform.localScale = new Vector3(anim.transform.localScale.x, inverted ? -1 : 1, anim.transform.localScale.z);

        // Gravity doesn't work as intended when the timeScales are not normal.
        magnitude *= timeScale * timeScale;
        magnitude /= GameManager.globalTimeScale * GameManager.globalTimeScale;

        // Each state has different needs.
        if (state == PlayerStates.Climb)
            gravityScale = 0.0f;
        else if (isInWater)
            gravityScale = 50.0f * magnitude * (inverted ? -1 : 1);
        else if (isInSand)
            gravityScale = 25.0f * magnitude * (inverted ? -1 : 1);
        else
            gravityScale = 100.0f * magnitude * (inverted ? -1 : 1);
    }

    public virtual void UpdateGameManager()
    {
        // All the GameManager needs from the player is their position for now.
        GameManager.playerPosition = transform.position;
    }

    public void Outro()
    {
        StartCoroutine(PlayOutro());
    }
    protected IEnumerator PlayIntro()
    {
        // Sets the right states and animation.
        SetState(PlayerStates.Paused);
        anim.Play("Intro");
        canBeHurt = false;
        canAnimate = false;
        anim.applyRootMotion = false;
        GameManager.roomFinishedLoading = false;

        yield return null;

        // Waits for the length of the animation.
        yield return new WaitForSeconds(1.1f);

        // Resets the player states.
        canAnimate = true;
        SetState(PlayerStates.Normal);
        canBeHurt = true;
        anim.applyRootMotion = true;
        GameManager.roomFinishedLoading = true;
    }
    protected IEnumerator PlayOutro()
    {
        canMove = false;
        canBeHurt = false;
        canAnimate = false;

        yield return new WaitForSeconds(1.0f);

        anim.applyRootMotion = false;
        anim.Play("Outro");
        audioStage.PlaySound(SFXLibrary.TpOut, true);

        yield return new WaitForSeconds(2.0f);

        GameManager.bossesActive = 0;

        anim.applyRootMotion = true;
        GameManager.GoToStageSelect();
    }

    public void Earthquake(float time)
    {
        StartCoroutine(KnockOff(time));
    }

    protected IEnumerator KnockOff(float time)
    {
        yield return null;

        canMove = false;
        canAnimate = false;
        body.velocity = Vector3.zero;

        anim.Play("Fallen");

        while (time > 0.0f)
        {
            time -= Time.fixedUnscaledDeltaTime;
            body.MovePosition(transform.position - right * 4.0f * Time.fixedDeltaTime * Time.timeScale);

            yield return new WaitForFixedUpdate();
        }

        canMove = true;
        canAnimate = true;
    }


    public bool isInSand
    {
        // Checks if the player is inside sand.
        get { return Physics2D.OverlapBox(transform.position + _center, new Vector2(width, height), 0.0f, 1 << 12) != null; }
    }
    public bool isInWater
    {
        // Checks if the player is in water.
        get { return Physics2D.OverlapBox(transform.position + _center, new Vector2(width, height), 0.0f, 1 << 4); }
    }
    public bool isGrounded
    {
        // Checks if the player is touching the ground.
        get
        {
            return ((body == null || body.velocity.y * up.y <= 0.0f) &&
                    Physics2D.OverlapBox(transform.position + _center - up * height * 0.6f,
                                        new Vector2(width * 0.95f, height * 0.25f), 0, 1 << 8) != null);
        }
    }
    public bool facesWall
    {
        // Checks if the player is touching a wall.
        get
        {
            return Physics2D.OverlapBox(transform.position + _center + right * width * 0.3f,
                                        new Vector2(width * 0.6f, height * 0.9f), 0, 1 << 8) != null;
        }
    }
    public bool canStand
    {
        // Checks if there is a solid where the player's standing collider should be.
        get
        {
            return Physics2D.OverlapBox(transform.position + _center + up * height * 0.375f,
                                        new Vector2(width * 0.5f, height * 0.45f), 0, 1 << 8) == null;
        }
    }
    // These three keep track of the player's state in the previous frame.
    protected bool wasGrounded;
    protected bool wasInWater;
    protected bool wasInSand;
}


/// <summary>
/// This class holds basic Health Bars the player will need in their UI.
/// Health Bar and Double Gear related UI is held in this class.
/// </summary>
[System.Serializable]
public class PlayerHealthbars
{
    [Header("Basic bars")]
    public Sprite emptyBar;
    public Sprite healthBar;
    public Sprite rushBar;

    [Header("Gears")]
    public Sprite gearBackground;
    public Sprite speedGear1;
    public Sprite speedGear2;
    public Sprite powerGear1;
    public Sprite powerGear2;
    public Sprite doubleGear1;
    public Sprite doubleGear2;

}

/// <summary>
/// This class holds the sound effects a player will need.
/// The names are self explanatory.
/// </summary>
[System.Serializable]
public class PlayerSFX
{
    public AudioClip land;
    public AudioClip shoot;
    public AudioClip charge;
    public AudioClip shootBig;

    public AudioClip hurt;
    public AudioClip death;
    public AudioClip TpOut;

    public AudioClip deflect;

    public AudioClip healthRecover;

    public AudioClip menuMove;
    public AudioClip menuOpen;

    public AudioClip pharaohShot;
    public AudioClip pharaohCharge;
    public AudioClip geminiLaser;
}