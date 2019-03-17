using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the Mega Buster Player Weapon Data.
/// 
/// The Mega Buster can be charged for up to two levels,
/// and has an extra fully charged shot for the Power Gear.
/// 
/// While named the 'Mega' Buster, any player can use it.
/// The player will also keep their original colors.
/// 
/// Reminder, this is the Weapon Data used by the player,
/// not the actual Lemon instance that appears in-game.
/// </summary>
public class PlWpDt_MegaBuster : Pl_WeaponData
{


    private float charge = 0.0f;

    // The Mega Buster doesn't require any special parameters in its constructor.
    public PlWpDt_MegaBuster(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }



    public override void Init()
    {

        base.Init();
        // The Mega Buster doesn't require a Weapon Energy Bar, so no full or empty bars need to be assigned.
        energyBarEmpty = null;
        energyBarFull = null;
        // Finds the weapon icons (selected and non-selected).
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon     = sprites[2 * 2];
        weaponIconGray = sprites[2 * 2 + 1];
    }
    public override void Press()
    {
        // Shoots, sets the player animation to shooting and plays a shoot sound.
        Shoot(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, 0);
        owner.shootTime = 0.2f;
        if (!owner.audioWeapon.isPlaying || owner.audioWeapon.clip != owner.SFXLibrary.shootBig)
            owner.audioWeapon.PlaySound(owner.SFXLibrary.shoot, false);
    }
    public override void Hold()
    {
        // Adds to the charge, without considering time slowing effects active.
        charge += Time.unscaledDeltaTime;
        // If the charge level has just gotten over 0.3f, the charge sound starts playing.
        if (charge > 0.3f && charge - Time.unscaledDeltaTime <= 0.3f)
            owner.audioWeapon.PlaySound(owner.SFXLibrary.charge, true);
    }
    public override void Release()
    {
        if (charge > 0.3f)
        {
            // If the Power Gear is active, shoots two shots.
            if (owner.gearActive_Power)
            {
                owner.StartCoroutine(ShootGear(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, charge));
                owner.shootTime = 0.5f;
            }
            // Else, if a semi charged shot or more needs to be shot,
            // the appropriate shot is shot.
            else
            {
                Shoot(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, charge);
                owner.shootTime = 0.2f;
            }

            // If a big shot is hot, a sound is played.
            // If not, the charge noise is cancelled.
            if (charge > 1.0f)
                owner.audioWeapon.PlaySound(owner.SFXLibrary.shootBig, true);
            else
                owner.audioWeapon.PlaySound(null, true);

        }
        charge = 0.0f;
    }
    public override void Cancel()
    {
        charge = 0.0f;
        owner.audioWeapon.PlaySound(null, true);
    }


    public override WeaponColors GetColors()
    {
        // If there are no charging colors, there is no need
        // to check the charge level and return the right colors.
        if (chargeColors == null)
            return baseColors;

        // If fully charged, the right colors are decided based on Unity's timer.
        if (charge > 1.0f)
        {
            if (chargeColors.Length >= 2)
            {
                int index = Mathf.FloorToInt((Time.unscaledTime % 0.2f) * 15.0f);
                return chargeColors[1, index];
            }
        }
        // If semi charged, the right colors are decided based on Unity's timer.
        else if (charge > 0.3f)
        {
            if (chargeColors.Length >= 1)
            {
                int index = Mathf.FloorToInt((Time.unscaledTime % 0.3f) * 10.0f);
                return chargeColors[0, index];
            }
        }
        // If not charged, returns the default colors.
        return baseColors;
    }


    protected void Shoot(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float charge)
    {
        // Finds the right shot based on the charge given.
        // The charge variable here is local, so you can lie
        // when you call Shoot() and just shoot fully charged
        // shots forever. This would be considered lying though. 
        Pl_Shot newShot = null;
        if (charge > 1.0f)
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/MegaManChargedShot"));
        else if (charge > 0.3f)
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/MegaManMidShot"));
        else
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/MegaManSmallShot"));

        // Finds the right position and orientation for the shot.
        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));
        // Sets the damage and speed of the shot.
        newShot.damage *= damageMultiplier;
        newShot.speed = speed;
    }

    protected IEnumerator ShootGear(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float charge)
    {
        // The Double Gear charged shot needs the player to freeze until both shots are shot.
        owner.canMove = false;
        owner.body.gravityScale = 0.0f;
        owner.body.velocity = Vector2.zero;

        // Shoots the first shot.
        Pl_Shot newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/MegaManChargedShot"));

        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));
        newShot.damage *= damageMultiplier;
        newShot.speed = speed;


        yield return new WaitForSecondsRealtime(0.05f);

        // Adds some knockback to the player.
        owner.body.gravityScale = 100.0f;
        owner.body.velocity = -owner.right * 70.0f + owner.up * 200.0f;

        // Shoots the second shot.
        newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/MegaManPoweredShot"));
        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));
        newShot.damage *= damageMultiplier;
        newShot.speed = speed;

        yield return new WaitForSeconds(0.35f);

        // The player can move again.
        owner.canMove = true;
    }

}
