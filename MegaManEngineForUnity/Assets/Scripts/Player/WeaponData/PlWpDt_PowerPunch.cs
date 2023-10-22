using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_PowerPunch : Pl_WeaponData
{


    private float charge = 0.0f;
    private float cooldown = 0.0f;
    private bool punching;

    // The Mega Buster doesn't require any special parameters in its constructor.
    public PlWpDt_PowerPunch(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
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

    public override void Press()
    {
        // Shoots, sets the player animation to shooting and plays a shoot sound.
        if (cooldown <= 0.0f)
        {
            Shoot(owner.right * owner.width * 1.3f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, 0, owner.gearActive_Power);
            owner.shootTime = 0.2f;
            if (!owner.audioWeapon.isPlaying || owner.audioWeapon.clip != owner.SFXLibrary.shootBig)
                owner.audioWeapon.PlaySound(owner.SFXLibrary.shoot, true);
        }
    }
    public override void Hold()
    {
        // Adds to the charge, without considering time slowing effects active.
        if (owner.gearActive_Power)
            charge += Time.unscaledDeltaTime * 3f;
        else
            charge += Time.unscaledDeltaTime;
        // If the charge level has just gotten over 0.3f, the charge sound starts playing.
        if (charge > 0.3f && charge - Time.unscaledDeltaTime <= 0.3f)
            owner.audioWeapon.PlaySound(owner.SFXLibrary.charge, true);
    }
    public override void Release()
    {
        if (charge > 0.3f)
        {
            owner.StartCoroutine(PlayPunchAnim(owner, charge));
            owner.shootTime = 0.2f;

            // If a big shot is hot, a sound is played.
            // If not, the charge noise is cancelled.
            if (charge > 1.0f)
                owner.audioWeapon.PlaySound(owner.SFXLibrary.shootBig, true);
            else
                owner.audioWeapon.PlaySound(null, true);

        }
        charge = 0.0f;
        if (cooldown <= 0.0f)
            cooldown = 0.2f;
    }
    public override void Cancel()
    {
        charge = 0.0f;
        owner.audioWeapon.PlaySound(null, true);
    }
    public override void Update()
    {
        base.Update();

        if (cooldown > 0.0f)
            cooldown -= Player.deltaTime;
        else if (punching)
        {
            if (owner.shootTime > 0 && owner.state == Player.PlayerStates.Normal && owner.isGrounded)
                owner.input.x = 0;

            if (cooldown > 0)
                cooldown -= Player.deltaTime;
        }
    }
    public override void OnGUI(float x, float y)
    {
        base.OnGUI(x, y);

        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);

        // Reads and displays the healthbar.
        Sprite healthBar = owner.bars.rushBar;
        Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
        Sprite emptyBar = owner.bars.emptyBar;
        Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
        for (int i = 0; i < 14; i++)
        {
            if (charge * 14 > i)
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 32f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
            else
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 32f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
        }
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
                WeaponColors colors = chargeColors[1, index];
                if (owner)
                {
                    switch (index)
                    {
                        case 0:
                            colors.colorDark = owner.defaultColors.colorLight;
                            colors.colorOutline = owner.defaultColors.colorDark;
                            break;
                        case 1:
                            colors.colorLight = owner.defaultColors.colorDark;
                            colors.colorOutline = owner.defaultColors.colorLight;
                            break;
                        case 2:
                            colors.colorOutline = owner.defaultColors.colorLight;
                            colors.colorLight = owner.defaultColors.colorDark;
                            break;
                    }
                }
                return colors;
            }
        }
        // If semi charged, the right colors are decided based on Unity's timer.
        else if (charge > 0.5f)
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

    protected void Shoot(Vector3 offset, bool shootLeft, int damageMultiplier, float speed, float charge, bool powerGear)
    {
        // Finds the right shot based on the charge given.
        // The charge variable here is local, so you can lie
        // when you call Shoot() and just shoot fully charged
        // shots forever. This would be considered lying though. 
        Pl_Shot newShot = null;
        if (powerGear)
        {
            if (charge > 1.0f) 
                newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PowerBigBoom"));
            else
                newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PowerShockwave"));
        }
        else
        {
            if (charge > 1.0f)
                newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PowerExplosion"));
            else if (charge > 0.5f)
                newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PowerShockwave"));
            else
                newShot = Object.Instantiate(Resources.Load<Pl_Shot>("Prefabs/PlayerWeapons/PowerPunch"));
        }

        // Finds the right position and orientation for the shot.
        newShot.transform.position = owner.transform.position + offset;
        if (shootLeft)
            newShot.transform.localScale = Vector3.Scale(newShot.transform.localScale, new Vector3(-1, 1, 1));
        // Sets the damage and speed of the shot.
        //newShot.damage *= damageMultiplier;
        //newShot.speed = speed;
    }
    private IEnumerator PlayPunchAnim(Player owner,  float charge)
    {
        cooldown = 0.4f;
        punching = true;
        owner.canAnimate = false;
        owner.body.velocity = Vector3.Project(owner.body.velocity, owner.transform.up);

        if (owner.state == Player.PlayerStates.Climb)
        {
            owner.anim.Play("ClimbPunch");
            owner.canMove = false;
        }
        else if (!owner.isGrounded)
        {
            owner.anim.Play("JumpPunch");
        }
        else
        {
            owner.anim.Play("StandPunch");
            owner.canMove = false;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        Shoot(owner.right * owner.width * 2.0f, Vector3.Angle(owner.right, Vector3.right) > 90, 1, 300, charge, owner.gearActive_Power);

        yield return new WaitForSecondsRealtime(0.1f);

        punching = false;
        owner.canMove = true;
        owner.canAnimate = true;
    }

}
