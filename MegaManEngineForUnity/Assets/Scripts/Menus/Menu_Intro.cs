using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Menu_Intro : Menu
{


    private GUISkin font;

    public AudioClip submit;
    public int index;

    private float countdown = 1.0f;
    private bool startCounting = false;


    public override void Start()
    {
        font = (GUISkin)Resources.Load("GUI/8BitFont", typeof(GUISkin));
    }
    public override void Update()
    {
        if (Input.GetButtonDown("Jump") && !startCounting)
        {
            startCounting = true;
            Helper.PlaySound(submit);
        }

        if (startCounting)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0.0f)
                Helper.GoToStage("Cutscene_Intro");
        }
    }
    public override void DrawGUI()
    {
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        int blockSize = (int)(Camera.main.pixelWidth / 16);

        font.label.fontSize = (int)(blockSize * 0.5625f);

        GUI.Label(new Rect(cmrBase.x + 1.5f * blockSize,
                   cmrBase.y + 10.0f * blockSize,
                   20.0f * blockSize,
                   10.0f * blockSize),
                   "Find and press the Jump button to start!",
                   font.label);

        font.label.fontSize = (int)(font.label.fontSize * 0.5f);

        GUI.Label(new Rect(cmrBase.x + 9.0f * blockSize,
           cmrBase.y + 13.0f * blockSize,
           20.0f * blockSize,
           10.0f * blockSize),
           "Contact MegaChibisX for bugs and comments.",
           font.label);
    }

}
