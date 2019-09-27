using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_MetalBlade : Pl_WeaponData
{

    private GameObject metalBlade;
    private GameObject metalWheel;


    public PlWpDt_MetalBlade(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[5];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[5 * 2];
        weaponIconGray = sprites[5 * 2 + 1];

        metalBlade = (GameObject)Resources.Load("Prefabs/PlayerWeapons/MetalBlade");
        metalWheel = (GameObject)Resources.Load("Prefabs/PlayerWeapons/MetalWheel");
    }

    public override void Press()
    {
        if (weaponEnergy > 0)
        {
            if (owner.gearActive_Power)
            {
                weaponEnergy -= 5f;
                ThrowBig(owner.input);
            }
            else
            {
                weaponEnergy -= 0.25f;
                Throw(owner.input);
            }
            weaponEnergy = Mathf.Clamp(weaponEnergy, 0f, 28f);
        }
    }

    private void Throw(Vector2 dir)
    {
        Debug.Log(dir);
        if (dir == Vector2.zero)
            dir = owner.right;
        dir.Normalize();
        dir.x *= owner.transform.localScale.x;
        dir.y *= owner.anim.transform.localScale.y;

        GameObject blade = Object.Instantiate(metalBlade);
        blade.transform.position = owner.transform.position + new  Vector3(dir.x, dir.y, -1f) * 8f;
        blade.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90f, Vector3.forward) * dir);

        owner.throwTime = 0.25f;
    }

    private void ThrowBig(Vector2 dir)
    {
        Debug.Log(dir);
        if (dir == Vector2.zero)
            dir = owner.right;
        dir.Normalize();
        dir.x *= owner.transform.localScale.x;
        dir.y *= owner.anim.transform.localScale.y;

        GameObject blade = Object.Instantiate(metalWheel);
        blade.transform.position = owner.transform.position + new Vector3(dir.x, dir.y, -1f) * 16f;
        blade.transform.rotation = Quaternion.LookRotation(Vector3.forward, Quaternion.AngleAxis(90f, Vector3.forward) * dir);

        owner.throwTime = 0.25f;
    }

}
