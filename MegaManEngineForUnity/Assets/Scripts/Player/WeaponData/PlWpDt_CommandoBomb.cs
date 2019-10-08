using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_CommandoBomb : Pl_WeaponData
{

    public GameObject bomb;
    public GameObject generator;

    private bool dropping = false;

    public PlWpDt_CommandoBomb(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[10];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[10 * 2];
        weaponIconGray = sprites[10 * 2 + 1];

        bomb = (GameObject)Resources.Load("Prefabs/PlayerWeapons/CmdBomb");
        generator = (GameObject)Resources.Load("Prefabs/PlayerWeapons/CmdExpGenerator");
    }


    public override void Press()
    {
        if (owner.slideTime > 0.0f)
            return;

        if (weaponEnergy > 0)
        {
            if (owner.gearActive_Power)
            {
                weaponEnergy -= 3f;
                owner.StartCoroutine(Drop());
            }
            else
            {
                weaponEnergy -= 3f;
                owner.shootTime = 0.2f;
                Shoot();
            }
            weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28f);
        }
    }

    private void Shoot()
    {
        GameObject newBomb = Object.Instantiate(bomb);
        newBomb.transform.position = owner.transform.position + owner.right * 8f + owner.up * 4;
        if (owner.anim.transform.localScale.x < 0)
            newBomb.transform.eulerAngles = Vector3.forward * 180f;
        else
            newBomb.transform.eulerAngles = Vector3.forward * 0f;
    }
    private  IEnumerator Drop()
    {   

        while (!owner.isGrounded)
        {
            yield return null;
        }

        GameObject gen = Object.Instantiate(generator);
        gen.transform.position = owner.transform.position - owner.up * owner.height * 0.75f;
        gen.transform.eulerAngles = Vector3.forward * (owner.up.y > 0f ? 0f : 180f);
    }

}
