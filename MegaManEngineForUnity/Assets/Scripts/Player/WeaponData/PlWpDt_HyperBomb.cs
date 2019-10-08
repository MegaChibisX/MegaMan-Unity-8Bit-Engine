using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_HyperBomb : Pl_WeaponData
{

    private GameObject hyperBomb;
    private GameObject bouncyBomb;

    public PlWpDt_HyperBomb(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[7];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[7 * 2];
        weaponIconGray = sprites[7 * 2 + 1];

        hyperBomb = (GameObject)Resources.Load("Prefabs/PlayerWeapons/HyperBomb");
        bouncyBomb = (GameObject)Resources.Load("Prefabs/PlayerWeapons/BouncyBomb");
    }

    public override void Press()
    {
        if (owner.slideTime > 0.0f)
            return;

        if (weaponEnergy > 0.0f)
        {
            weaponEnergy -= 2.0f;
            Throw(owner.gearActive_Power);
        }
        weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28f);
    }

    private void Throw(bool power)
    {
        GameObject bomb = Object.Instantiate(power ? bouncyBomb : hyperBomb);
        bomb.transform.position = owner.transform.position;

        Rigidbody2D body = bomb.GetComponent<Rigidbody2D>();
        if (owner.input.y < 0)
            body.velocity = owner.right * 250f + owner.up * 250f;
        else
            body.velocity = owner.right * 150f + owner.up * 350f;
        body.gravityScale = 100f * owner.up.y;
    }

}
