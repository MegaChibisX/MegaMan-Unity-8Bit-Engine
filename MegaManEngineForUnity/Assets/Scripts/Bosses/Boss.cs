using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base for every boss,
/// as every boss or subboss should inherit from
/// this class.
/// By default, this class has references for a basic healthbar,
/// a question if the fight has started and a timer for invisibility
/// after a hit.
/// </summary>
public class Boss : Enemy
{


    public Sprite healthBarFull;
    public Sprite healthBarEmpty;
    protected bool fightStarted;
    protected float invisTime = 0.0f;
    


    protected virtual void OnGUI()
    {
        // Draws the health bar if the fight has started.
        if (fightStarted)
        {
            float x = Camera.main.pixelWidth / 256.0f;
            float y = Camera.main.pixelHeight / 218.0f;
            Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);

            Sprite healthBar = healthBarFull;
            Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                    healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
            Sprite emptyBar = healthBarEmpty;
            Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                    emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
            for (int i = 0; i < 28; i++)
            {
                if (health > i)
                    GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 40f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
                else
                    GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 40f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
            }
        }
    }

    public override void Damage(float dmg, bool ignoreInvis)
    {
        // If the fight hasn't started, the boss can't get hurt.
        // Bosses linger in the arena before they appear, so this way
        // the player can't kill them before entering their arena.
        if (!fightStarted)
            return;
        base.Damage(dmg, ignoreInvis);
    }


    public virtual void StartFight()
    {
        StartCoroutine(PlayIntro());
    }
    public override void Kill(bool makeItem)
    {
        // Reduces the number of active bosses in the GameManager,
        // and then registers death as a normal enemy.
        GameManager.bossesActive--;
        base.Kill(false);
    }

    public virtual IEnumerator PlayIntro()
    {
        // Boss intros should override this function.
        yield return null;
    }

}
