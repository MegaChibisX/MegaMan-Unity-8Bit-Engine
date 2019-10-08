using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_StarCrash : Pl_WeaponData
{

    public GameObject starShield;
    public GameObject meteorShield;

    private PlWp_StarShield activeShield;

    public PlWpDt_StarCrash(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[6];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[6 * 2];
        weaponIconGray = sprites[6 * 2 + 1];

        starShield = (GameObject)Resources.Load("Prefabs/PlayerWeapons/StarShield");
        meteorShield = (GameObject)Resources.Load("Prefabs/PlayerWeapons/MeteorShield");
    }

    public override void Press()
    {
        if (activeShield != null)
        {
            weaponEnergy--;
            activeShield.Throw();
            activeShield = null;
        }
        else if (weaponEnergy > 0)
        {
            owner.throwTime = 0.2f;
            if (owner.gearActive_Power)
            {
                Spawn(true);
                weaponEnergy -= 4f;
            }
            else
            {
                Spawn(false);
                weaponEnergy -= 3f;
            }
        }
        weaponEnergy = Mathf.Clamp(weaponEnergy, 0f, 28f);
    }
    public override void Cancel()
    {
        if (activeShield != null)
            Object.Destroy(activeShield.gameObject);
    }

    private  void Spawn(bool power)
    {
        activeShield = Object.Instantiate(power ? meteorShield.gameObject : starShield.gameObject).GetComponent<PlWp_StarShield>();
        activeShield.transform.position = owner.transform.position;
        activeShield.owner = owner;
    }


}
