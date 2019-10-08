using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the Gemini Laser Player Weapon Data.
/// Reminder, this is the Weapon Data used by the player,
/// not the actual Laser instance that appears in-game.
/// </summary>
public class PlWpDt_GeminiLaser : Pl_WeaponData {


    // There needs to be a limit to the amount of Gemini Lasers
    // active in the stage at a time. Other than the potentially
    // overpowered weapon, the sound is horrible. Just trust me on this.
    private int lasersActive = 0;
    // The Prefab of the Gemini laser.
    private GameObject laserInstance;
    // The Prefab of the Power Gear Gemini Laser.
    private GameObject laserBouncyInstance;
    // The *annoying* noise the laser does when it hits a wall.
    private AudioClip geminiNoise;

    // The Gemini Laser doesn't require any extra parameters in its constructor.
    public PlWpDt_GeminiLaser(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();
        // Finds the laser Prefabs from the Resources folder.
        laserInstance = (GameObject)Resources.Load("Prefabs/PlayerWeapons/GeminiLaser", typeof(GameObject));
        laserBouncyInstance = (GameObject)Resources.Load("Prefabs/PlayerWeapons/GeminiBouncy", typeof(GameObject));

        // Finds the bars (empty and full) and weapon icons (selected and non-selected).
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[4];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[4 * 2];
        weaponIconGray = sprites[4 * 2 + 1];
    }

    public override void Press()
    {
        // Doesn't attack if sliding or dashing
        if (owner.slideTime > 0.0f)
            return;

        // Gemini Laser can be fired as long as the weapon energy is more than zero,
        // even if the player has less then the required weapon energy for a shot.
        if (weaponEnergy > 0)
        {
            // If the power gear is active, 4 smaller shots are shot.
            if (owner.gearActive_Power)
            {
                // Reads the *annoying* sound from the player.
                if (owner != null)
                    geminiNoise = owner.SFXLibrary.geminiLaser;

                // Sets the player to show their shooting animations.
                owner.shootTime = 0.2f;
                // Consumes weapon energy and prevents it from going under zero.
                weaponEnergy -= 2;
                weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28);
                // Plays the *annoying* Gemini Laser sound.
                owner.audioWeapon.PlaySound(owner.SFXLibrary.geminiLaser, true);

                // Starts creating shots from the highest angle and goes down for each shot.
                Vector2 shotAngle = Quaternion.AngleAxis(105, Vector3.forward) * owner.right;
                for (int i = 0; i < 4; i++)
                {
                    // Creates an instance of the shot and angles it appropriately.
                    GameObject o = Object.Instantiate(laserBouncyInstance);
                    o.transform.position = owner.transform.position + owner._center + owner.right * owner.width;
                    o.transform.rotation = Quaternion.LookRotation(Vector3.forward, shotAngle);
                    o.GetComponent<SpriteRenderer>().flipX = owner.right.x > 0;

                    // Gives the shot the noise and right order. This way, 
                    // only the first shot to appear plays a sound when it 
                    // hits a wall. This works better with the regular Gemini Noise.
                    PlWp_GeminiShot shot;
                    if ((shot = o.GetComponent<PlWp_GeminiShot>()) != null)
                    {
                        shot.geminiNoise = geminiNoise;
                        shot.shotOrder = i;
                    }
                    // Calculates the angle the next shot should use, based on the angle of this shot.
                    shotAngle = Quaternion.AngleAxis(-10, Vector3.forward) * shotAngle;
                }
            }
            else
            {
                // Reads the *annoying* sound from the player.
                if (owner != null)
                    geminiNoise = owner.SFXLibrary.geminiLaser;


                // Sets the player to show their shooting animations.
                owner.shootTime = 0.2f;
                // Consumes weapon energy and prevents it from going under zero.
                weaponEnergy -= 1;
                weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28);
                // Plays the *annoying* Gemini Laser sound.
                owner.audioWeapon.PlaySound(owner.SFXLibrary.geminiLaser, true);

                // A Gemini Laser consists of 3 pieces. Each of these pieces
                // spawns on its own, and only the first one will play a sound
                // when it hits a wall.
                for (int i = 0; i < 3; i++)
                {
                    // Spawns the Laser Fragment and positions it in the right place.
                    GameObject o = Object.Instantiate(laserInstance);
                    o.transform.position = owner.transform.position + owner._center + owner.right * (owner.width + i * 6);
                    o.GetComponent<SpriteRenderer>().flipX = owner.right.x > 0;

                    // Gives the shot the noise and right order.
                    // This way, only the first shot to appear 
                    // plays a sound when it hits a wall.
                    PlWp_GeminiShot shot;
                    if ((shot = o.GetComponent<PlWp_GeminiShot>()) != null)
                    {
                        shot.geminiNoise = geminiNoise;
                        shot.shotOrder = i;
                    }
                }
            }
        }
    }

}
