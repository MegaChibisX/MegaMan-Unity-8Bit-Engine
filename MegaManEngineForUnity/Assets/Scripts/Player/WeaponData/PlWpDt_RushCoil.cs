using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_RushCoil : PlWpDt_MegaBuster
{

    public PlWp_RushCoil coilPrefab;
    public PlWp_RushCoil coil;

    private bool waitingForContact = false;

    public PlWpDt_RushCoil(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[62];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[62 * 2];
        weaponIconGray = sprites[62 * 2 + 1];

        coilPrefab = ((GameObject)Resources.Load("Prefabs/PlayerWeapons/RushCoil", typeof(GameObject))).GetComponent<PlWp_RushCoil>();
    }

    public override void Press()
    {
        if (coil == null)
        {
            RaycastHit2D hit;
            Vector3 targetPos = owner.transform.position + owner.right * 32f;
            if (hit = Physics2D.Raycast(targetPos, -owner.up, 128f, 1 << 8))
                targetPos = hit.point;


            coil = Object.Instantiate(coilPrefab);
            coil.transform.position = targetPos;
            coil.transform.localScale = owner.anim.transform.localScale;
            waitingForContact = true;        }
        else
        {
            base.Press();
        }
    }
    public override void Update()
    {
        if (coil)
        {
            if (coil.madeContact && waitingForContact)
            {
                waitingForContact = false;
                weaponEnergy -= 2f;
            }
        }

        base.Update();
    }

    public override void Cancel()
    {
        if (coil)
            Object.Destroy(coil.gameObject);
    }


}
