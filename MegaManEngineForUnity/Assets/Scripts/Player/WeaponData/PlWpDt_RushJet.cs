using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_RushJet : PlWpDt_MegaBuster
{

    public Ri_RushJet jetPrefab;
    public Ri_RushJet jet;

    private bool waitingForContact = false;

    public PlWpDt_RushJet(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[63];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[63 * 2];
        weaponIconGray = sprites[63 * 2 + 1];

        jetPrefab = ((GameObject)Resources.Load("Prefabs/PlayerWeapons/RushJet", typeof(GameObject))).GetComponent<Ri_RushJet>();
    }
    public override void Start()
    {
        base.Start();

        owner.StartCoroutine(Update());
    }

    public override void Press()
    {
        if (jet == null)
        {
            RaycastHit2D hit;
            Vector3 targetPos = owner.transform.position + owner.right * 32f;
            if (hit = Physics2D.Raycast(targetPos, -owner.up, 128f, 1 << 8))
                targetPos = hit.point;


            jet = Object.Instantiate(jetPrefab);
            jet.transform.position = targetPos;
            jet.transform.localScale = owner.anim.transform.localScale;
            waitingForContact = true;
        }
        else
        {
            base.Press();
        }
    }
    // The normal Update() method won't work here, because weapon updates are ignored when in a Ride.
    public new IEnumerator Update()
    {
        while (true)
        {
            if (owner.currentWeapon.menuName != menuName)
                yield break;

            if (jet)
            {
                if (jet.rider)
                {
                    weaponEnergy -= 2f * Time.deltaTime;
                    if (weaponEnergy <= 0.0f)
                    {
                        weaponEnergy = 0.0f;
                        owner.Dismount();
                        jet.Kill();
                    }
                }
            }
            yield return null;
        }

        base.Update();
    }

    public override void Cancel()
    {
        if (jet)
        {
            jet.Dismount();
            Object.Destroy(jet.gameObject);
        }
    }


}
