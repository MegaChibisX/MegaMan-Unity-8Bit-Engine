using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the Pharaoh Shot Player Weapon Data.
/// 
/// Reminder, this is the Weapon Data used by the player,
/// not the actual Pharaoh Shot instance that appears in-game.
/// </summary>
public class PlWpDt_PharaohShot : Pl_WeaponData {


    // The Pharaoh Shot has a charged version,
    // but it doesn't change the player's colors.
    private float charge = 0.0f;

    // The charged shot needs to be kept track of.
    private Pl_Shot shotBig;
    // bigShotShouldBeActive checks if the big shot has already been created.
    // If it is true, but shotBig = null, the big shot has hit an enemy and disappeared.
    private bool bigShotShouldBeActive = false;
    // Without this, once a big shot is destroyed by hitting an enemy,
    // another one would immediately spawn and take its place.
    private bool bigShotCanBeCreated = false;
    // If the player just wants to use charged shot after charged shot,
    // they don't need to shoot the smaller ones all the time,
    // which will consume more energy than they will need.
    private float smallShotCooldown = 0.0f;

    // The Pharaoh Shot doesn't require any special parameters in its constructor.
    public PlWpDt_PharaohShot(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();
        // Finds the bars (empty and full) and weapon icons (selected and non-selected).
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[3];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[3 * 2];
        weaponIconGray = sprites[3 * 2 + 1];
    }
    public override void Start()
    {
        // The shot shouldn't be active at the beginning.
        bigShotShouldBeActive = false;
    }
    public override void Update()
    {
        // If the shot is expected to be active but it hits an enemy,
        // weapon energy should be consumed and the shot
        // shouldn't still be expected to be active.
        if (bigShotShouldBeActive && shotBig == null)
        {
            weaponEnergy -= 2;
            bigShotShouldBeActive = false;
        }
        // Reduces the cooldown for small shots.
        if (smallShotCooldown > 0.0f)
            smallShotCooldown -= Time.unscaledDeltaTime;
    }

    public override void Press()
    {
        if (weaponEnergy > 0)
        {
            // If the Power Gear is active, just 3 simple big shots are shot.
            if (owner.gearActive_Power)
            {
                owner.StartCoroutine(ShootPowered(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, 0));
                weaponEnergy -= 4;
            }
            else
            {
                if (smallShotCooldown <= 0.0f)
                {
                    // Shots a small Pharaoh Shot at the appropriate direction.
                    Shoot(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, 0);
                    // Sets the appropriate player animations and consumes energy.
                    owner.throwTime = 0.2f;
                    weaponEnergy--;
                }
                // Registers that a big charged shot can be created again.
                bigShotCanBeCreated = true;
                owner.audioWeapon.PlaySound(owner.SFXLibrary.pharaohShot, true);
            }
        }
    }
    public override void Hold()
    {
        if (owner.gearActive_Power)
        {
            // Can't have a big shot with the Power Gear.
            charge = 0.0f;
            if (shotBig != null)
                Release();
            return;
        }
        // Adds to the charge.
        charge += Time.unscaledDeltaTime;
        // If the charge is over a threshold, there is no active big shot,
        // the player has enough weapon energy, there is no expected shot to be active
        // and a big shot hasn't been just consumed by an enemy, spawns a shot.
        if (charge >= 0.2f && shotBig == null && weaponEnergy > 0 && !bigShotShouldBeActive && bigShotCanBeCreated)
        {
            // Creates a big shot at the right position and makes it track the player.
            shotBig = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PharaohShotBig"));
            shotBig.transform.position = owner.transform.position + owner.up * 26.0f;
            shotBig.GetComponent<Misc_FollowTransform>().displacement = owner.up * 26.0f;
            shotBig.GetComponent<Misc_FollowTransform>().target = owner.transform;
            // A big shot should be active now.
            bigShotShouldBeActive = true;
            // No big shots can be created again until
            // the fire button is released.
            bigShotCanBeCreated = false;
            // Plays a shoot sound.
            owner.audioWeapon.PlaySound(owner.SFXLibrary.pharaohCharge, true);
        }
    }
    public override void Release()
    {
        // If there is enough charge and a big shot hasn't been consumed,
        // shoots the big shot at the desired angle.
        if (charge > 0.8f && shotBig)
        {
            Shoot(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, charge);
            owner.throwTime = 0.2f;
            weaponEnergy -= 2f;
            owner.audioWeapon.PlaySound(owner.SFXLibrary.pharaohShot, true);
            shotBig = null;
            smallShotCooldown = 0.2f;
        }
        // If there isn't enough charge,
        // the big shot gets destroyed.
        else if (shotBig)
        {
            Object.Destroy(shotBig.gameObject);
            smallShotCooldown = 0.2f;
        }
        // Sets the variables tat keep track of the big shot.
        bigShotShouldBeActive = false;
        bigShotCanBeCreated = false;
        charge = 0.0f;
    }
    public override void Cancel()
    {
        // Destroys any big shots, resets the charge and stops any sounds that are playing.
        if (shotBig != null)
            Object.Destroy(shotBig.gameObject);
        bigShotShouldBeActive = false;
        charge = 0.0f;
        owner.audioWeapon.PlaySound(null, true);
    }


    public override WeaponColors GetColors()
    {
        return baseColors;
    }


    protected void Shoot(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float charge)
    {
        // If there is no charge, creates a big shot,
        // and if there is enough charge, it selects the big
        // shot and tells it to no longer follow the player.
        // If there is some charge but not enough, or the big shot
        // has been destroyed, nothing happens.
        Pl_Shot newShot = null;
        if (charge == 0.0f)
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PharaohShotsmall"));
        else if (charge > 0.8f && shotBig)
        {
            newShot = shotBig;
            Object.Destroy(newShot.GetComponent<Misc_FollowTransform>());
            shotBig = null;
        }
        else
            return;


        // Both the big shot and small shot act the same,
        // so the newShot can be either.
        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));

        if ((Input.GetAxisRaw("Vertical") > 0.5f && !shootLeft) ||
            (Input.GetAxisRaw("Vertical") < -0.5f && shootLeft))
            newShot.transform.eulerAngles += Vector3.forward * 45.0f;
        if ((Input.GetAxisRaw("Vertical") < -0.5f && !shootLeft) ||
            (Input.GetAxisRaw("Vertical") > 0.5f && shootLeft))
            newShot.transform.eulerAngles -= Vector3.forward * 45.0f;

        // Sets the damage and speed, and starts the shot.
        newShot.damage *= damageMultiplier;
        newShot.speed = speed;
        newShot.Start();
    }

    protected IEnumerator ShootPowered(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float charge)
    {
        // Prevents the player from moving
        // and puts them in a throwing animation.
        owner.canMove = false;
        owner.body.gravityScale = 100.0f;
        owner.body.velocity = Vector2.zero;
        owner.throwTime = 0.1f;
        yield return null;

        // Each shot is a simple big shot, with a small delay between shots.
        for (int i = 0; i < 3; i++)
        {
            // Sets the velocity, position and orientation of each shot.
            owner.body.velocity = -owner.right * 70.0f + owner.up * 100.0f;
            Pl_Shot newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PharaohShotPowered"));
            newShot.transform.position = owner.transform.position + offset;
            if (shootLeft)
                newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));

            // Sets the damage of and starts the shot.
            newShot.damage *= damageMultiplier;
            newShot.Start();

            // Adds a small delay after every shot.
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Lets the player move again.
        owner.canMove = true;
        owner.body.gravityScale = 100.0f;
    }

}
