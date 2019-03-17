using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This was going to be in the demo, but it never got used. If it's still present in the release, blame a lazy or forgetful MegaChibisX.
/// </summary>
public class Menu_Controls : Menu {



    private Player owner;
    private Texture background;
    private GUISkin font;

    public void Start()
    {
        background = (Texture)Resources.Load("Sprites/Menus/BackgroundEmpty", typeof(Texture));
        font = (GUISkin)Resources.Load("GUI/8BitFont", typeof(GUISkin));
    }
    public override void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {

        }
    }
    
    public override void DrawGUI()
    {
        
    }

}
