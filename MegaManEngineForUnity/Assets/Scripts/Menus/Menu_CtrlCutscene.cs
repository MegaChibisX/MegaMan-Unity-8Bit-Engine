using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu_CtrlCutscene : MonoBehaviour
{

    public string nextRoom;
    public Menu_Cutscene scene;



    protected void Start()
    {
        GameManager.checkpointCamera_LeftCenter = Vector2.zero;
        GameManager.checkpointCamera_MaxRightMovement = 0;
        GameManager.checkpointCamera_MaxUpMovement = 0;
        scene.anim = GetComponentInChildren<Animator>();
        StartCoroutine(PlayCutscene(scene.cutsceneSegments));
    }

    public IEnumerator PlayCutscene(CutsceneItem[] items)
    {
        // A cutscene should consist out of multiple segments.
        for (int i = 0; i < items.Length; i++)
        {
            CutsceneItem item = items[i];

            // Sets the animator state. Each cutscene animator should have an animIndex variable
            // which tells it which part of the cutscene should be played.
            // The parts are separated based on the text displayed.
            if (scene.anim)
                scene.anim.SetInteger("animIndex", item.animIndex);
            scene.guiText = "";

            if (item.shakeGround)
                GameManager.ShakeCamera(item.nextTextDelay, 2f, false);

            yield return new WaitForSecondsRealtime(item.nextTextDelay);

            // Sets up the image to display.
            scene.guiImage = item.image;
            scene.guiImageOffset = item.offset;
            scene.guiImageSize = item.imageSizeOnViewport;

            if (item.changeMusic && CameraCtrl.instance)
                CameraCtrl.instance.PlayMusic(item.newMusic);

            // Displays every piece of text on its own.
            if (item.text.Length > 0) {
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
                            scene.guiText = item.text[j];

                            while (Input.GetButton("Jump"))
                                yield return null;
                        }
                        scene.guiText = item.text[j].Substring(0, maxChars);

                        yield return new WaitForSecondsRealtime(0.02f);
                    }

                    // Waits for the Jump button to be pressed.
                    while (!Input.GetButtonDown("Jump"))
                    {
                        yield return null;
                    }
                }

                // Fades image out.
                float fadeTime = item.fadeTime;
                float timeDiv = 1f / fadeTime;
                if (fadeTime == 0)
                    timeDiv = 1;
                while (fadeTime > 0.0f)
                {
                    float color = fadeTime * timeDiv;
                    scene.guiColor = new Color(color, color, color, 1);
                }
                fadeTime = item.fadeTime;
                while (fadeTime > 0.0f)
                {
                    float color = 1f - fadeTime * timeDiv;
                    scene.guiColor = new Color(color, color, color, 1);
                }
                scene.guiColor = Color.white;

                yield return null;
            }
            yield return null;
        }
        // Finishes the cutscene.
        GameManager.GoToStageSelect();
    }

    protected void OnGUI()
    {
        // Draws the cutscene.
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        float pixelSize = (Camera.main.pixelWidth / 256f);
        scene.font.label.fontSize = (int)(pixelSize * 8);

        if (scene.guiBackground)
            GUI.DrawTexture(new Rect(cmrBase.x, cmrBase.y, Camera.main.pixelWidth, Camera.main.pixelHeight), scene.guiBackground, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
        if (scene.guiImage)
        {
            float ratio = scene.guiImage.height / (float)scene.guiImage.width;
            GUI.DrawTexture(new Rect(cmrBase.x + scene.guiImageOffset.x,
                                     cmrBase.y + scene.guiImageOffset.y,
                                     Camera.main.pixelWidth  * scene.guiImageSize,
                                     Camera.main.pixelHeight * scene.guiImageSize * ratio),
                                     scene.guiImage, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
        }

        GUI.Label(new Rect(cmrBase.x + pixelSize * 56f,
                           cmrBase.y + pixelSize * 160f,
                           pixelSize * 144,
                           pixelSize * 48), scene.guiText, scene.font.label);
    }


}
