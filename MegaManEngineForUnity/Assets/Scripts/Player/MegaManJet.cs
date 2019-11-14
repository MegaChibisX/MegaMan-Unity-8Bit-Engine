using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("MegaMan/Allies/MegaMan Jet")]
public class MegaManJet : Player
{

    public bool shouldBeInJet = false;
    public float jetTime = 2.0f;



    protected override void OnGUI()
    {
        // x and y are about the size of one in-game pixel.
        // cmrBase is the base of the playable viewport, right next
        // to the left black border.
        float x = Camera.main.pixelWidth / 256.0f;
        float y = Camera.main.pixelHeight / 218.0f;
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);


        // Reads and displays the jet fuel bar.
        Sprite healthBar = bars.rushBar;
        Rect healthBarRect = new Rect(healthBar.rect.x / healthBar.texture.width, healthBar.rect.y / healthBar.texture.height,
                                healthBar.rect.width / healthBar.texture.width, healthBar.rect.height / healthBar.texture.height);
        Sprite emptyBar = bars.emptyBar;
        Rect emptyBarRect = new Rect(emptyBar.rect.x / emptyBar.texture.width, emptyBar.rect.y / emptyBar.texture.height,
                                emptyBar.rect.width / emptyBar.texture.width, emptyBar.rect.height / emptyBar.texture.height);
        for (int i = 0; i < 14; i++)
        {
            if (jetTime * 7 > i)
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 32f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), healthBar.texture, healthBarRect);
            else
                GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + x * 32f, cmrBase.y + y * (72 - i * 2), x * 8, y * 2), emptyBar.texture, emptyBarRect);
        }

        base.OnGUI();
    }

    protected override void Land()
    {
        // If in ground, there is no way the player should be flying.
        shouldBeInJet = false;

        base.Land();
    }
    protected override void HandleInput_Movement()
    {

        // If there are no more jumps left for the player, go jet.
        if (Input.GetButtonDown("Jump"))
        {
            if (jetTime > 0.0f && !isGrounded && jumpsLeft <= 0f)
                shouldBeInJet = true;
        }
        // If the player releases the  jump button, don't jet.
        else if (Input.GetButtonUp("Jump"))
        {
            shouldBeInJet = false;
        }

        // Add jet fuel when on ground.
        if (isGrounded)
        {
            jetTime = Mathf.MoveTowards(jetTime, 2.0f, Time.fixedDeltaTime * 5f);
        }

        // Add y velocity to fly and use fuel.
        if (Input.GetButton("Jump") && shouldBeInJet && jetTime > 0.0f)
        {
            float forceMulti = 1f;
            forceMulti *= timeScale * timeScale;
            forceMulti /= GameManager.globalTimeScale;

            body.velocity += (gravityInverted ? Vector2.down : Vector2.up) * 25f * forceMulti;

            jetTime -= Time.deltaTime;
            return;
        }
        else
            base.HandleInput_Movement();
    }

    protected override void Animate()
    {
        // When the played animation is handled externally,
        // like during a combo or cutscene, default animations are ignored.
        if (!canAnimate)
            return;

        if (Input.GetButton("Jump") && shouldBeInJet && jetTime > 0.0f)
        {
            string nameSuffix = "";

            if (shootTime > 0.0f)
                nameSuffix = "Shoot";
            else if (throwTime > 0.0f)
                nameSuffix = "Throw";

            anim.Play("Jet" + nameSuffix);
            return;
        }

        base.Animate();
    }

    public override void SetGravity(float magnitude, bool inverted)
    {
        // Lower gravity can make the jet act weird. Make sure this doesn't happen.
        if (Input.GetButton("Jump") && shouldBeInJet && jetTime > 0.0f)
        {
            base.SetGravity(1, inverted);
        }
        else
            base.SetGravity(magnitude, inverted);
    }

}
