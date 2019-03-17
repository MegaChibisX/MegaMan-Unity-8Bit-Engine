using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script respawns enemies when they enter the screen.
/// This way enemy deaths aren't permanent.
/// </summary>
[AddComponentMenu("MegaMan/Enemy/_Blueprint")]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyBlueprint : MonoBehaviour {


    // blueprint should be the enemy that will spawn when needed.
    public GameObject blueprint;
    // blueprintInstance is the instance of the enemy active in the scene.
    private GameObject blueprintInstance;
    // The sprite of the Blueprint, which is shown for convenience.
    public SpriteRenderer sprite;

    // All of these are related to the enemy.
    [SerializeField]
    private Vector2 center;
    [SerializeField]
    private Vector2 size;

    [SerializeField]
    private bool upToDate = false;
    public bool gotOutsideVew = true;


    private void Start()
    {
        // Calculates the spawn bounds of the blueprint if they haven't been updated.
        if (!upToDate)
            CalculateBounds();
        sprite.enabled = false;
    }
    private void Update()
    {
        if (!blueprintInstance)
        {
            // If the Blueprint enters the view, it spawns an enemy.
            if (gotOutsideVew && InView(false))
                SpawnEnemy();
            // If the Blueprint is no longer in view, allow it to enter the view again.
            else if (!gotOutsideVew && !InView(false))
                gotOutsideVew = true;
        }
        else
        {
            // If the enemy is outside the view, resets the Blueprint.
            if (!InView_Instance(false) && blueprintInstance.GetComponent<Enemy>())
            {
                blueprintInstance.GetComponent<Enemy>().Despawn();
            }
        }

    }
    private void OnDrawGizmosSelected()
    {
        // Draws the area around the Blueprint where the enemy will be visible.
        Gizmos.color = upToDate ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3)center, (Vector3)size * 0.5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + (Vector3)center, (Vector3)size + new Vector3(192, 192, 0));

        Gizmos.color = InView(false) ? Color.cyan : Color.blue;
        Gizmos.DrawCube(transform.position + (Vector3)center + Vector3.up * size.y * 25f + Vector3.right * 5f, Vector3.one * 3.5f + Vector3.forward * 100.0f);

        Gizmos.color = InView(true) ? Color.yellow : new Color(1.0f, 0.5f, 0.0f);
        Gizmos.DrawCube(transform.position + (Vector3)center + Vector3.up * size.y * 25f - Vector3.right * 5f, Vector3.one * 3.5f + Vector3.forward * 100.0f);
    }                                   


    public bool SpawnEnemy()
    {
        // Spawns the enemy, if there isn't one already in the scene.
        if (blueprintInstance)
            return false;

        blueprintInstance = Instantiate(blueprint);
        blueprintInstance.transform.parent = transform.parent;
        blueprintInstance.transform.position = transform.position;
        blueprintInstance.transform.rotation = transform.rotation;
        blueprintInstance.transform.localScale = transform.localScale;
        gotOutsideVew = false;

        return true;
    }

    public void CalculateBounds()
    {
        // Finds the area of the enemy. Comparing the bounds of the enemy with the bounds
        // of the camera is how it is calculated when the enemy should spawn.
        if (!blueprint)
        {
            upToDate = false;
            return;
        }

        // Rect has the attributes x, y, width, height, but here, we will use them as xLeft, yDown, xRight, yUp
        Rect viewArea = new Rect(0, 0, 0, 0);


        // Checks through all the colliders and finds the outer bounds of the combined colliders.
        BoxCollider2D[] cols = blueprint.GetComponentsInChildren<BoxCollider2D>();
        for (int i = 0; i < cols.Length; i++)
        {
            Rect colRect = new Rect(cols[i].offset - cols[i].size, cols[i].offset + cols[i].size);
            colRect.width -= colRect.x;
            colRect.height -= colRect.y;
            if (i == 0)
            {
                viewArea = colRect;
            }
            else
            {

                if (colRect.x < viewArea.x)
                    viewArea.x = colRect.x;
                else if (colRect.width > viewArea.width)
                    viewArea.width = colRect.width;

                if (colRect.y < viewArea.y)
                    viewArea.y = colRect.y;
                else if (colRect.height > viewArea.height)
                    viewArea.height = colRect.height;
            }
        }

        // Sets the sprite of the Blueprint to the sprite of the enemy. Good for development.
        sprite = GetComponent<SpriteRenderer>();
        SpriteRenderer r = blueprint.GetComponentInChildren<SpriteRenderer>();
        if (r)
            sprite.sprite = r.sprite;
        else
            sprite.sprite = null;

        // Saves the bounds of the camera to the Blueprint.
        center = viewArea.center;
        size = viewArea.size;
        upToDate = true;
        name = "Blueprint (" + blueprint.name + ")";
    }
    public bool InView(bool fullInView)
    {
        CameraCtrl cmr = CameraCtrl.instance;

        if (!cmr)
            return false;

        int withinBounds = 0;

        // Checks if the Blueprint is in view.
        if (fullInView)
        {
            if (transform.position.x + center.x + size.x < cmr.transform.position.x + 104)
                withinBounds++;
            if (transform.position.x + center.x - size.x > cmr.transform.position.x - 104)
                withinBounds++;
            if (transform.position.y + center.y + size.y < cmr.transform.position.y + 104)
                withinBounds++;
            if (transform.position.y + center.y - size.y > cmr.transform.position.y - 104)
                withinBounds++;
        }
        else
        {
            if (transform.position.x + center.x - size.x < cmr.transform.position.x + 104)
                withinBounds++;
            if (transform.position.x + center.x + size.x > cmr.transform.position.x - 104)
                withinBounds++;
            if (transform.position.y + center.y - size.y < cmr.transform.position.y + 104)
                withinBounds++;
            if (transform.position.y + center.y + size.y > cmr.transform.position.y - 104)
                withinBounds++;
        }

        return withinBounds == 4;
    }
    public bool InView_Instance(bool fullInView)
    {
        CameraCtrl cmr = CameraCtrl.instance;

        if (!cmr || !blueprintInstance)
            return false;

        int withinBounds = 0;

        // Checks if the enemy instance is in view.
        if (fullInView)
        {
            if (blueprintInstance.transform.position.x + center.x + size.x < cmr.transform.position.x + 104)
                withinBounds++;
            if (blueprintInstance.transform.position.x + center.x - size.x > cmr.transform.position.x - 104)
                withinBounds++;
            if (blueprintInstance.transform.position.y + center.y + size.y < cmr.transform.position.y + 104)
                withinBounds++;
            if (blueprintInstance.transform.position.y + center.y - size.y > cmr.transform.position.y - 104)
                withinBounds++;
        }
        else
        {
            if (blueprintInstance.transform.position.x + center.x - size.x < cmr.transform.position.x + 104)
                withinBounds++;
            if (blueprintInstance.transform.position.x + center.x + size.x > cmr.transform.position.x - 104)
                withinBounds++;
            if (blueprintInstance.transform.position.y + center.y - size.y < cmr.transform.position.y + 104)
                withinBounds++;
            if (blueprintInstance.transform.position.y + center.y + size.y > cmr.transform.position.y - 104)
                withinBounds++;
        }

        return withinBounds == 4;
    }

}
