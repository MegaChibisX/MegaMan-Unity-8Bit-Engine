using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlWpDt_BlackHoleBomb : Pl_WeaponData
{

    private GameObject blackHoleBaby;
    private GameObject blackHoleNuke;

    private float cooldownTime = 0.0f;


    public PlWpDt_BlackHoleBomb(Player _owner, string _menuName, WeaponColors _baseColors, WeaponColors[,] _chargeColors) : base(_owner, _menuName, _baseColors, _chargeColors)
    {

    }

    public override void Init()
    {
        base.Init();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Menus/Bars");
        energyBarEmpty = sprites[0];
        energyBarFull = sprites[9];
        sprites = Resources.LoadAll<Sprite>("Sprites/Menus/WeaponIcons");
        weaponIcon = sprites[9 * 2];
        weaponIconGray = sprites[9 * 2 + 1];

        
        blackHoleBaby = (GameObject)Resources.Load("Prefabs/PlayerWeapons/BlackHoleBaby");
        blackHoleNuke = (GameObject)Resources.Load("Prefabs/PlayerWeapons/BlackHoleNuke");
    }

    public override void Update()
    {
        base.Update();

        if (!owner.paused)
            cooldownTime -= Time.unscaledDeltaTime;
    }

    public override void Press()
    {
        if (cooldownTime >= 0.0f || weaponEnergy <= 0.0f || owner.slideTime > 0.0f)
            return;


        if (owner.gearActive_Power)
        {
            weaponEnergy -= 14f;
            cooldownTime = 4.0f;
            owner.StartCoroutine(FookinNuke());
            GameManager.ShakeCamera(4f, 4f, false);
        }
        else
        {
            weaponEnergy -= 4f;
            owner.shootTime = 0.2f;
            cooldownTime = 5.0f;
            Shoot();
            GameManager.ShakeCamera(5f, 1f, false);
        }

        weaponEnergy = Mathf.Clamp(weaponEnergy, 0, 28);
    }

    private void Shoot()
    {
        GameObject hole = Object.Instantiate(blackHoleBaby);
        hole.transform.position = owner.transform.position + owner.right * 10f;
        hole.transform.localScale = new Vector3(owner.anim.transform.localScale.x, 1, 1);
    }
    private IEnumerator FookinNuke()
    {
        GameObject nuke = Object.Instantiate(blackHoleNuke);
        nuke.transform.position = CameraCtrl.instance.transform.position + Vector3.forward * 5f;
        nuke.transform.parent = CameraCtrl.instance.transform;

        yield return new WaitForSecondsRealtime(5.0f);

        Object.Destroy(nuke);
    }


}
