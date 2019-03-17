using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Menus do NOT derive from MonoBehaviour, which means functions like "Update" won't run automatically by Unity,
/// but instead we need to call them from another object.
/// </summary>
public class Menu
{

    public Menu menuParent;


    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    public virtual void DrawGUI() { }


}
