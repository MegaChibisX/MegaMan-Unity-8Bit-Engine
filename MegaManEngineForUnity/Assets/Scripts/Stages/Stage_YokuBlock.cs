using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Yoku Blocks, or disappearing blocks.
/// This is the scripr that every individual block
/// should have, and how their patterns should be decided
/// by Stage_YokuController.cs.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Yoku\\Disappearing Block")]
[RequireComponent(typeof(SpriteRenderer))]
public class Stage_YokuBlock : MonoBehaviour {

    private SpriteRenderer sprite;
    private Collider2D col;

    // Animation-related, as the yoku blocks don't have very complex animations.
    public int framesPerSecond = 6;
    private float frameTime = 0.0f;
    private bool hidden = true;
    private int index = 0;

    // The frames for each animation.
    public Sprite[] appearFrames;
    public Sprite[] disappearFrames;


    private void Start()
    {
        // The Yoku Blocks need to change the sprites through script.
        sprite = GetComponent<SpriteRenderer>();

        // Error check.
        col = GetComponentInChildren<Collider2D>();
        if (!col)
        {
            Debug.LogWarning("There is no collider at Yoku Block " + name + "!");
        }

        if (appearFrames.Length == 0)
        {
            Debug.LogWarning("There is no Yoku Block to show at " + name + "!");
        }
    }

    private void Update()
    {
        if (hidden)
        {
            // If there is no yoku block animation, or the animation has ended, hides the block and exits.
            if (disappearFrames.Length == 0 || index == disappearFrames.Length)
            {
                if (sprite.enabled)
                    sprite.enabled = false;
                if (col.enabled)
                    col.enabled = false;

                return;
            }

            // Plays the hidding animation.
            if ((int)frameTime < disappearFrames.Length)
            {
                float prevTime = frameTime;
                frameTime += Time.deltaTime * framesPerSecond;
                if ((int)prevTime != (int)frameTime)
                {
                    index = (int)frameTime;
                    if (index < disappearFrames.Length)
                        sprite.sprite = disappearFrames[index];
                    else
                        return;
                }
            }
        }
        else
        {
            
            // If there are no animation frames, no animation is played.
            if (appearFrames.Length == 0 || index == appearFrames.Length)
            {
                return;
            }

            // Plays the appearing animation.
            if ((int)frameTime < appearFrames.Length - 1)
            {
                float prevTime = frameTime;
                frameTime += Time.deltaTime * framesPerSecond;
                if ((int)prevTime != (int)frameTime)
                {
                    index = (int)frameTime;
                    if (index < appearFrames.Length)
                        sprite.sprite = appearFrames[index];
                    else
                        return;
                }
            }
        }
    }


    public void ShowBlock()
    {
        // Shows the block.
        index = 0;
        frameTime = 0.0f;
        hidden = false;

        sprite.enabled = true;
        col.enabled = true;
    }

    public void HideBlock()
    {
        // Hides the block.
        index = 0;
        frameTime = 0.0f;
        hidden = true;

        sprite.enabled = true;
        col.enabled = true;
    }



}
