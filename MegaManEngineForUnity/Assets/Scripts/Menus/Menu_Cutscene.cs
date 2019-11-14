using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script used to display cutscenes. Cutscenes are an overglorified menu.
/// </summary>
[System.Serializable]
public class Menu_Cutscene : Menu
{
    public bool isActive = false;

    public CutsceneItem[] cutsceneSegments;
    public Animator anim;

    public GUISkin font;
    public Texture2D guiBackground;
    
    public string guiText;
    public Texture2D guiImage;
    public Color guiColor;
    public Vector2 guiImageOffset;
    public float guiImageSize;

    public  IEnumerator PlayCutscene(CutsceneItem[] items)
    {
        // A cutscene should consist out of multiple segments.
        for (int i = 0; i < items.Length; i++)
        {
            CutsceneItem item = items[i];

            // Sets the animator state. Each cutscene animator should have an animIndex variable
            // which tells it which part of the cutscene should be played.
            // The parts are separated based on the text displayed.
            if (anim)
                anim.SetInteger("animIndex", item.animIndex);

            if (item.shakeGround)
                GameManager.ShakeCamera(item.nextTextDelay + 0.5f, 2f, false);

            yield return new WaitForSecondsRealtime(item.nextTextDelay);

            // Sets up the image to display.
            guiImage = item.image;
            guiImageOffset = item.offset;
            guiImageSize = item.imageSizeOnViewport;

            // Displays every piece of text on its own.
            for (int j = 0; j < item.text.Length; j++)
            {
                // The text slowly appears, unless the Jump button is pressed.
                int maxChars = 0;
                bool awaitRelease = Input.GetButton("Jump");

                while (maxChars < item.text[j].Length)
                {
                    maxChars += 1;

                    if (awaitRelease && !Input.GetButton("Jump"))
                        awaitRelease = false;
                    if (Input.GetButton("Jump") && !awaitRelease)
                    {
                        maxChars = item.text[j].Length;
                        guiText = item.text[j];

                        while (Input.GetButton("Jump"))
                            yield return null;
                    }
                    guiText = item.text[j].Substring(0, maxChars);

                    yield return new WaitForSecondsRealtime(0.02f);
                }

                // Waits for the Jump button to be pressed.
                while (!Input.GetButtonDown("Jump"))
                {
                    yield return null;
                }

                // Fades image out.
                float fadeTime = item.fadeTime;
                float timeDiv = 1f / fadeTime;
                if (fadeTime == 0)
                    timeDiv = 1;
                while (fadeTime > 0.0f)
                {
                    float color = fadeTime * timeDiv;
                    guiColor = new Color(color, color, color, 1);
                }
                fadeTime = item.fadeTime;
                while (fadeTime > 0.0f)
                {
                    float color = 1f - fadeTime * timeDiv;
                    guiColor = new Color(color, color, color, 1);
                }
                guiColor = Color.white;


                yield return null;
            }
            yield return null;
        }
        // Finishes the cutscene.
        Exit();
    }


    public override void Start()
    {
        // Plays the cutscene at Start.
        isActive = true;
        Player.instance.StartCoroutine(PlayCutscene(cutsceneSegments));
    }
    public override void Exit()
    {
        // Stops the cutscene.
        isActive = false;
    }
    public override void DrawGUI()
    {
        // Draws the cutscene.
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        float pixelSize = (Camera.main.pixelWidth / 256f);
        font.label.fontSize = (int)(pixelSize * 8);

        if (guiBackground)
            GUI.DrawTexture(new Rect(cmrBase.x, cmrBase.y, Camera.main.pixelWidth, Camera.main.pixelHeight), guiBackground, ScaleMode.StretchToFill, true, 0, guiColor, 0, 0);
        if (guiImage)
        {
            float ratio = guiImage.height / (float)guiImage.width;
            GUI.DrawTexture(new Rect(cmrBase.x + guiImageOffset.x,
                                     cmrBase.y + guiImageOffset.y,
                                     Camera.main.pixelWidth  * guiImageSize,
                                     Camera.main.pixelHeight * guiImageSize * ratio),
                                     guiImage);
        }

        GUI.Label(new Rect(cmrBase.x + pixelSize * 56f,
                           cmrBase.y + pixelSize * 160f,
                           pixelSize * 144,
                           pixelSize * 48), guiText, font.label);
    }


}

[System.Serializable]
public class CutsceneItem
{
    // Segment of the cutscene, keeps track of the display image and some text.
    public Texture2D image;
    public Vector2 offset;
    public int animIndex;
    public bool playWithText;
    public float nextTextDelay;
    public float fadeTime;
    public float imageSizeOnViewport;
    public bool shakeGround;
    public AudioClip newMusic;
    public bool changeMusic;

    public string[] text;
}