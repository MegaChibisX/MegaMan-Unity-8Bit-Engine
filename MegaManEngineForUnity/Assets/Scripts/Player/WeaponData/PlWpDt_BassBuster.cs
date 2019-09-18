using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_BassBuster : Pl_WeaponData
{

    public float cooldown = 0;


    public PlWpDt_BassBuster(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
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
        weaponIcon = sprites[2 * 2];
        weaponIconGray = sprites[2 * 2 + 1];
    }
    public override void Hold()
    {
        if (cooldown > 0)
            return;

        float angle = 0;
        Bass ownerBass = owner.GetComponent<Bass>();
        if (ownerBass != null)
        {
            ownerBass.shootDir = Bass.ShootDir.Right;
            if  (owner.input.y != 0)
            {
                if (owner.input.y < 0)
                {
                    angle = -45f;
                    ownerBass.shootDir = Bass.ShootDir.DownRight;
                }
                else if (owner.input.x != 0)
                {
                    angle = 45f;
                    ownerBass.shootDir = Bass.ShootDir.UpRight;
                }
                else
                {
                    angle = 90f;
                    ownerBass.shootDir = Bass.ShootDir.Up;
                }
            }
        }

        bool lookLeft = Vector3.Angle(owner.right, Vector3.right) > 90;
        Vector3 offset = Quaternion.AngleAxis(angle * (lookLeft ? -1 : 1), Vector3.forward) * owner.right * owner.width * 1.3f;

        if (angle == 0)
            offset.y += -1f;
        else if (angle == -45)
            offset.x += 6;
        else if (angle == 45)
            offset.y -= 8;
        else
            offset.x += 4;

        Shoot(offset, lookLeft, 1, 300, angle);
        owner.shootTime = 0.2f;
        owner.audioWeapon.PlaySound(owner.SFXLibrary.shoot, true);

        if (owner.gearActive_Power)
            cooldown = 0.04f;
        else
            cooldown = 0.1f;
    }
    public override void Update()
    {
        if (owner.input.x != 0 && owner.canMove && !owner.paused)
        {
            owner.anim.transform.localScale = new Vector3(owner.input.x > 0 ? 1 : -1, owner.anim.transform.localScale.y, owner.anim.transform.localScale.z);
            owner.lastLookingLeft = owner.anim.transform.localScale.x < 0;
        }

        // Stops Bass from moving and shooting.
        if (owner.shootTime > 0 && owner.state == Player.PlayerStates.Normal && owner.isGrounded)
            owner.input.x = 0;

        if (cooldown > 0)
            cooldown -= Player.deltaTime;
    }


    protected void Shoot(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float angle)
    {
        Pl_Shot newShot = null;
        if (owner.gearActive_Power)
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/BassShotPierce"));
        else
            newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/BassShot"));

        // Finds the right position and orientation for the shot.
        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));
        newShot.transform.localRotation = Quaternion.AngleAxis(angle * (shootLeft ? -1 : 1), Vector3.forward);
        // Sets the damage and speed of the shot.
        newShot.damage *= damageMultiplier;
        newShot.speed = speed;
    }


}
