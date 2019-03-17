using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class all Player Weapon Data use.
/// Player Weapon Data is what the player uses to decide
/// the abilities and colors the player will have.
/// For the in-game weapons, see Pl_Weapon.cs.
/// 
/// This class also keeps track of all the Weapon Data
/// player can use. If a new Weapon Data class is made,
/// it needs to be registered in the WeaponList, 
/// or else it will not be usable by
/// conventional weapon selection means.
/// </summary>
[System.Serializable]
public class Pl_WeaponData {


    // This is the list of all the weapon data the player can use.
    // If you make a new weapon, it needs to be registed here before
    // it can be selected from a menu or given to a player.
    //
    // Also, it needs to be given a name in 'public enum Weapons',
    // which has to match its position in the WeaponList, like
    // the examples below. The item 'Length' in the Weapons enum
    // needs to always be last.
    public enum Weapons { MegaBuster, PharaohShot, GeminiLaser, Length }
    // The default parameters for each Pl_WeaponData are explained in this class's constructor.
    public static Pl_WeaponData[] WeaponList =
    {
        new PlWpDt_MegaBuster(null, "M. BUSTER", new WeaponColors(
                                            new Color(0,0,0,0), new Color(0,0,0,0), new Color(0,0,0,0)),
                                            new WeaponColors[,]
                                            {
                                                {
                                                new WeaponColors(new Color(0f/255f, 160f/255f, 255f/255f, 0), new Color(0f/256f, 97f/256f, 255f/255, 0), new Color(168f/255f, 0f/255f, 32f/255f)),
                                                new WeaponColors(new Color(0f/255f, 160f/255f, 255f/255f, 0), new Color(0f/256f, 97f/256f, 255f/255, 0), new Color(228f/255f, 0f/255f, 88f/255f)),
                                                new WeaponColors(new Color(0f/255f, 160f/255f, 255f/255f, 0), new Color(0f/256f, 97f/256f, 255f/255, 0), new Color(248f/255f, 88f/255f, 152f/255f))
                                                },
                                                {
                                                new WeaponColors(new Color(5f/256f, 5f/256f, 5f/255), new Color(0f/256f, 232f/256f, 216f/255), new Color(0f/255f, 120f/255f, 248f/255f)),
                                                new WeaponColors(new Color(0f/256f, 120f/256f, 248f/255f), new Color(5f/256f, 5f/256f, 5f/255), new Color(0f/255f, 0f/255f, 0f/255f)),
                                                new WeaponColors(new Color(0f/256f, 120f/256f, 248f/255f), new Color(188f/256f, 188f/256f, 188f/255), new Color(0f/255f, 0f/255f, 0f/255f))
                                                }
                                            }
                                        ),
        new PlWpDt_PharaohShot(null, "P. SHOT", new WeaponColors(new Color(240f/256f, 208f/256f, 176f/255),
                                                                 new Color(248f/255f, 120f/255f, 88f/255f),
                                                                 Color.black),
                                                                 null),
        new PlWpDt_GeminiLaser(null, "G. LASER", new WeaponColors(new Color(248f/256f,248f/256f, 248f/256f),
                                                                  new Color(60f/256f, 180f/256f, 255f/256f),
                                                                  new Color(0,0,0)),
                                                                  null)
    };

    public Player owner;
    // The basic colors the player will have
    public WeaponColors baseColors;
    // chargeColors[number of charge levels, number of different charge colors]
    public WeaponColors[,] chargeColors;

    public float weaponEnergy = 28;
    public float maxWeaponEnergy = 28;

    public string menuName;

    public Sprite energyBarFull;
    public Sprite energyBarEmpty;
    public Sprite weaponIcon;
    public Sprite weaponIconGray;
    public Sprite menuBarFull;
    public Sprite menuBarEmpty;


    // Each Player Weapon Data has a few variables by default,
    // which always need to be assigned. Each class can have
    // additional arguments in its constuctor, but these
    // will always need to be assigned.
    public Pl_WeaponData(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors)
    {
        // owner is the Player who is currently using a weapon.
        // In the WeaponList this will always be null, as it will
        // be assigned by the player themselves when they equip this weapon.
        owner = _owner;
        // baseColors are the Weapon Colors the player will use
        // when they are equiped with this weapon.
        // If the alpha of one of these colors is 0, the player
        // will use their default color instead.
        baseColors = _baseColors;
        // If the weapon can be charged, the colors the player will use
        // in each charge level can be assigned here.
        // If not, this can just be null instead.
        chargeColors = _chargeColors;
        // This is the name the weapon will show in the pause menu.
        // Try to keep it short!
        menuName = _menuName;
    }


    // Handles events when the weapon is first given to the player.
    public virtual void Init()
    {
        // Reads the sprites the weapon will use from the Resources folder.
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/BarsMenu");
        menuBarEmpty = sprites[0];
        menuBarFull  = sprites[1];
    }
    // Handles events when the weapon is selected by the player.
    public virtual void Start()
    {

    }
    // Handles events in each frame the weapon is active.
    public virtual void Update()
    {

    }
    // Handles events when the fire button is pressed.
    public virtual void Press()
    {

    }
    // Handles events while the fire button is held.
    public virtual void Hold()
    {

    }
    // Handles events when the fire button is released.
    public virtual void Release()
    {

    }
    // Handles events when the weapon needs to be forcefully cancelled.
    public virtual void Cancel()
    {
        if (owner != null)
            owner.audioWeapon.PlaySound(null, true);
    }

    // Handles the UI of each weapon. Usually the only thing handled
    // by this is the weapon energy bar. If a weapon energy bar with 28 units
    // is the only thing that needs to be shown, then the weapon doesn't need
    // to override this function.
    public virtual void OnGUI(float x, float y)
    {
        if (!energyBarFull || !energyBarEmpty)
            return;

        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);

        Sprite healthBar = energyBarFull;
        Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
        Sprite emptyBar = energyBarEmpty;
        Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
        for (int i = 0; i < 28; i++)
        {
            if (weaponEnergy > i)
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
            else
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 16f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
        }
    }

    // The colors the player will be using when the weapon is active.
    public virtual WeaponColors GetColors()
    {
        return baseColors;
    }

    // This struct keeps track of the player's colors when they
    // use a weapon. If the alpha of the color is 0, the player
    // will use their default colors for that specific area.
    [System.Serializable]
    public struct WeaponColors
    {
        // The lighter details of the player.
        public Color colorLight;
        // The darker details of the player.
        public Color colorDark;
        // The outline of the player.
        public Color colorOutline;

        public WeaponColors(Color _light, Color _dark, Color _outline)
        {
            colorLight = _light;
            colorDark = _dark;
            colorOutline = _outline;
        }
    }

}
