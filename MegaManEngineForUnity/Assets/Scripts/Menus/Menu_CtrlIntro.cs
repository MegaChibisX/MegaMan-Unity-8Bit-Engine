using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_CtrlIntro : MonoBehaviour
{


    public Menu_Intro menu;

    private void Start()
    {
        menu.Start();
    }
    private void Update()
    {
        menu.Update();
    }
    private void OnGUI()
    {
        menu.DrawGUI();
    }

}
