using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the script that manages a section with Yoku blocks.
/// Each yokuParents should be a GameObject that contains all the YokuBlocks
/// that get activated in a cycle. Each yokuParent stays active for 2 cycles,
/// then disappears until it is its time to appear again.
/// </summary>
[AddComponentMenu("MegaMan/Stage/Yoku Controller")]
public class Stage_YokuController : MonoBehaviour {


    // The Yoku Block sound.
    public AudioClip blockSound;

    // The yoku block bunches, and the length of every cycle.
    public GameObject[] yokuParents;
    public float timePerCycle = 1.0f;
    private int index = 0;

    // If the player is inside this area, the sound plays. If not,
    // the player is too far away to hear the noise.
    public Rect viewArea;


    private void Start()
    {
        // Sets the view area and starts the cycle.
        SetViewArea();
        StartCoroutine(YokuSchedule());
    }

    private void SetBlocks(int i, bool active)
    {
        // Error check.
        if (yokuParents.Length == 0)
        {
            Debug.LogWarning("Here are no Yoku Blocks for the Controller " + name + " to activate!");
            return;
        }

        // Shows or hides all the blocks in the selected parent.
        i = i % yokuParents.Length;

        foreach (Stage_YokuBlock block in yokuParents[i].GetComponentsInChildren<Stage_YokuBlock>())
        {
            if (active)
                block.ShowBlock();
            else
                block.HideBlock();
        }
    }
    private void SetViewArea()
    {
        // Sets the arena where the blocks are expected to be visible.

        // Rect has the attributes x, y, width, height, but here, we will use them as xLeft, yDown, xRight, yUp
        viewArea = new Rect(0,0,0,0);

        for (int i = 0; i < yokuParents.Length; i++)
        {
            int j = 0;
            foreach (Stage_YokuBlock b in yokuParents[i].GetComponentsInChildren<Stage_YokuBlock>())
            {
                if (i == 0 && j == 0)
                {
                    viewArea = new Rect(b.transform.position.x - 8, b.transform.position.y - 8, b.transform.position.x + 8, b.transform.position.y + 8);
                }
                else
                {
                    if (b.transform.position.x - 8 < viewArea.x)
                        viewArea.x = b.transform.position.x - 8;
                    else if (b.transform.position.x + 8 > viewArea.width)
                        viewArea.width = b.transform.position.x + 8;

                    if (b.transform.position.y - 8 < viewArea.x)
                        viewArea.y = b.transform.position.y - 8;
                    else if (b.transform.position.y + 8 > viewArea.height)
                        viewArea.height = b.transform.position.y + 8;
                }
                j++;
            }
        }

        viewArea = new Rect(viewArea.x - 128, viewArea.y - 120, viewArea.width + 128, viewArea.height + 120);
    }
    private bool PlayerInBounds()
    {
        return (GameManager.playerPosition.x > viewArea.x && GameManager.playerPosition.x < viewArea.width &&
                GameManager.playerPosition.y > viewArea.y && GameManager.playerPosition.y < viewArea.height);
    }

    private IEnumerator YokuSchedule()
    {
        // Error check.
        if (yokuParents.Length == 0)
        {
            Debug.LogWarning("Here are no Yoku Blocks for the Controller " + name + " to activate!");
            yield break;
        }


        // Shows the first blocks.
        yield return new WaitForSeconds(timePerCycle);
        if (PlayerInBounds())
            Helper.PlaySound(blockSound);
        SetBlocks(index, true);
        index++;
        // Shows the second blocks.
        yield return new WaitForSeconds(timePerCycle);
        if (PlayerInBounds())
            Helper.PlaySound(blockSound);
        SetBlocks(index, true);
        index++;

        // Shows the next row of blocks and hides the previous one
        while (true)
        {
            yield return new WaitForSeconds(timePerCycle);

            if (PlayerInBounds())
                Helper.PlaySound(blockSound);
            SetBlocks(index - 2, false);
            SetBlocks(index, true);
            index++;
        }

    }


}
