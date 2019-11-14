using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_Controller : MonoBehaviour
{

    public Menu_StageSelect menu;



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
    private void OnDrawGizmos()
    {
        menu.OnDrawGizmos();
    }


}
