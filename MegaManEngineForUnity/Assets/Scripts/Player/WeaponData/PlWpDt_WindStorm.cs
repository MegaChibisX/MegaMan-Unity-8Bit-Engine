using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_WindStorm : Pl_WeaponData
{


    public GameObject windStorm;
    public GameObject lootStorm;

    public PlWpDt_WindStorm(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[8];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[8 * 2];
        weaponIconGray = sprites[8 * 2 + 1];

        windStorm = (GameObject)Resources.Load("Prefabs/PlayerWeapons/WindStorm");
        lootStorm = (GameObject)Resources.Load("Prefabs/PlayerWeapons/LootStorm");
    }


    public override void Press()
    {
        if (owner.slideTime > 0.0f)
            return;

        if (weaponEnergy > 0.0f)
        {
            owner.shootTime = 0.2f;

            if (owner.gearActive_Power)
                weaponEnergy -= 6.0f;
            else
                weaponEnergy -= 1.0f;
            Shoot(owner.gearActive_Power);
            weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28f);
        }
    }

    private void Shoot(bool power)
    {
        GameObject tornado = Object.Instantiate(power ? lootStorm : windStorm);
        tornado.transform.position = owner.transform.position;
        tornado.transform.localScale = owner.anim.transform.localScale;
        if (owner.anim.transform.localScale.y < 0)
            tornado.GetComponent<Rigidbody2D>().gravityScale = -100f;
    }

}
