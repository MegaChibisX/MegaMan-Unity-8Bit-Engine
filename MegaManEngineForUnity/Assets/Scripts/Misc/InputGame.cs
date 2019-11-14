using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This was never used.
/// </summary>
public static class InputGame
{

    public enum Keys { Horizontal, Vertical, Fire1, Fire2, Jump, Slide, WeaponPlus, WeaponMinus, GearPlus, GearMinus }

    public static bool GetKeyDown(Keys key)
    {
        KeyValuePair<string, int> pair = GetAxisValue(key);
        int axis = pair.Value;
        string name = pair.Key;

        return false;
    }

    private static KeyValuePair<string, int> GetAxisValue(Keys key)
    {
        int axis = 0;
        string name = "";
        switch (key)
        {
            case Keys.Fire1:
                axis = 0;
                name = "Fire1";
                break;
            case Keys.Fire2:
                axis = 0;
                name = "Fire2";
                break;
            case Keys.GearMinus:
                axis = -1;
                name = "GearSwitch";
                break;
            case Keys.GearPlus:
                axis = 1;
                name = "GearSwitch";
                break;
            case Keys.Horizontal:
                axis = 0;
                name = "Horizontal";
                break;
            case Keys.Jump:
                axis = 0;
                name = "Jump";
                break;
            case Keys.Slide:
                axis = 0;
                name = "Slide";
                break;
            case Keys.Vertical:
                axis = 0;
                name = "Vertical";
                break;
            case Keys.WeaponMinus:
                axis = -1;
                name = "WeaponSwitch";
                break;
            case Keys.WeaponPlus:
                axis = 1;
                name = "WeaponSwitch";
                break;
        }

        return new KeyValuePair<string, int>(name, axis);
    }

}
