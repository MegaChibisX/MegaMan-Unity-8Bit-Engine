using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This script is used by the Pause Menu.
/// The Pause Menu consists of multiple sections, with one active at a time.
/// </summary>
public class Menu_Pause : Menu
{

    // The currect selected item in each section of the menu.
    private Player owner;
    private Vector3Int selectIndex_Weapon;
    private Vector3Int selectIndex_Player;
    private Vector3Int selectIndex_RecItems;
    private Vector3Int selectIndex_UtItems;
    private Vector3Int selectIndex_LowerText;

    // The background and font of the menu.
    private Texture background;
    private GUISkin font;

    // The sprites of the items in the menu.
    //private Sprite[] playerSprites;
    private PlayerData[] playerData;
    private Sprite[] recItemSprites;
    private Sprite[] utItemSprites;
    private string[] lowerTexts;

    // The sections of the menu.
    public enum SelectModes { Weapons, Players, RecoveryItems, UtilityItems, LowerText }
    public SelectModes mode;

    private float inputCooldown;
    private Vector2 lastInput;


    public void Start(Player player)
    {
        // There needs to be a Player as a reference.
        owner = player;
        if (owner == null)
        {
            Debug.LogWarning("There is no player for this pause Menu!");
        }

        // The active item in each section.
        selectIndex_Weapon = new Vector3Int(owner.currentWeaponIndex % 2, owner.currentWeaponIndex / 2, 0);
        selectIndex_Player = Vector3Int.zero;
        selectIndex_RecItems = Vector3Int.zero;
        selectIndex_UtItems = Vector3Int.zero;
        selectIndex_LowerText = Vector3Int.zero;

        // The background and font of the menu.
        background = (Texture)Resources.Load("Sprites/Menus/MenuDefault", typeof(Texture));
        font = (GUISkin)Resources.Load("GUI/8BitFont", typeof(GUISkin));

        // Gets sprites from the GameManager for each player.
        //playerSprites = new Sprite[GameManager.availablePlayers.Count];
        playerData = new PlayerData[] { new PlayerData(GameManager.Players.MegaMan),
                                         new PlayerData(GameManager.Players.ProtoMan),
                                         new PlayerData(GameManager.Players.Bass),
                                         new PlayerData(GameManager.Players.MegaManJet),
                                         new PlayerData(GameManager.Players.MegaManPower),
                                        new PlayerData(GameManager.Players.X)};

        // Reads the sprites of a sprite sheet from the Resources and assigns the right ones to the right sprites.
        recItemSprites = new Sprite[(int)GameManager.RecoveryItems.Length];

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Stage/Items");
        recItemSprites[0] = sprites[17];
        recItemSprites[1] = sprites[18];
        recItemSprites[2] = sprites[19];
        recItemSprites[3] = sprites[20];
        recItemSprites[4] = sprites[50];
        recItemSprites[5] = sprites[14];

        utItemSprites = new Sprite[(int)GameManager.RecoveryItems.Length];
        utItemSprites[0] = sprites[33];
        utItemSprites[1] = sprites[34];
        utItemSprites[2] = sprites[35];
        utItemSprites[3] = sprites[36];

        // The misc text options.
        lowerTexts = new string[3];
        lowerTexts[0] = "RETRY";
        lowerTexts[1] = "EXIT";
        lowerTexts[2] = "ALT+F4";

        Helper.PlaySound(owner.SFXLibrary.menuOpen);
    }
    public override void Start()
    {
        // Just a warning for the forgetful.
        Debug.LogError("'void Start()' should not be used with 'Menu_Pause'. Use 'void Start(Player player)' instead!");
    }

    public override void Update()
    {

        if (new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                        Mathf.Round(Input.GetAxisRaw("Vertical"))) !=
            lastInput)
            inputCooldown = 0.0f;
        else if (inputCooldown > 0.0f)
            inputCooldown -= Time.unscaledDeltaTime;

        // Handles input based on the active segment.
        switch (mode)
        {
            case SelectModes.Weapons:
                InputWeapon();
                break;
            case SelectModes.Players:
                InputPlayer();
                break;
            case SelectModes.RecoveryItems:
                InputRecItems();
                break;
            case SelectModes.UtilityItems:
                InputUtItems();
                break;
            case SelectModes.LowerText:
                InputLowerText();
                break;
        }


        // Uses current item or section.
        if (Input.GetButtonDown("Start"))
        {
            switch (mode)
            {
                // Sets a new player.
                case SelectModes.Players:
                    PlayerData data = playerData[selectIndex_Player.x];

                    Player newPlayer = Object.Instantiate(GameManager.GetPlayerPrefabPath(data.player)).GetComponent<Player>();
                    Player oldPlayer = owner;
                    newPlayer.body = newPlayer.GetComponent<Rigidbody2D>();

                    newPlayer.transform.position = oldPlayer.transform.position;
                    newPlayer.transform.rotation = oldPlayer.transform.rotation;
                    newPlayer.transform.localEulerAngles = oldPlayer.transform.localEulerAngles;
                    newPlayer.transform.parent = oldPlayer.transform.parent;

                    CameraCtrl.instance.player = newPlayer.transform;
                    newPlayer.useIntro = false;
                    owner = newPlayer;

                    newPlayer.RefreshWeaponList();
                    newPlayer.health = oldPlayer.health;
                    newPlayer.SetWeapon(oldPlayer.currentWeaponIndex);
                    foreach (Pl_WeaponData wpn in Pl_WeaponData.WeaponList)
                        wpn.owner = newPlayer;

                    Object.Destroy(oldPlayer.gameObject);
                    Exit();

                    break;
                // Sets the player's current weapon to the selected one.
                case SelectModes.Weapons:
                    owner.SetWeapon(selectIndex_Weapon.y * 2 + selectIndex_Weapon.x);
                    Exit();
                    break;
                // Uses a recovery item.
                case SelectModes.RecoveryItems:
                    switch (selectIndex_RecItems.x)
                    {
                        case (int)GameManager.RecoveryItems.ETank:
                            owner.StartCoroutine(Heal(100, true, false, GameManager.RecoveryItems.ETank));
                            break;
                        case (int)GameManager.RecoveryItems.WTank:
                            owner.StartCoroutine(Heal(100, false, true, GameManager.RecoveryItems.WTank));
                            break;
                        case (int)GameManager.RecoveryItems.MTank:
                            owner.StartCoroutine(Heal(100, true, true, GameManager.RecoveryItems.MTank));
                            break;
                        case (int)GameManager.RecoveryItems.LTank:
                            owner.StartCoroutine(Heal(100, false, false, GameManager.RecoveryItems.LTank));
                            break;
                        case (int)GameManager.RecoveryItems.RedBullTank:
                            owner.StartCoroutine(Heal(10, true, true, GameManager.RecoveryItems.RedBullTank));
                            break;
                    }
                    break;
                // Uses a utility item. Not implemented yet.
                case SelectModes.UtilityItems:

                    break;
                // Each item does something unique.
                case SelectModes.LowerText:
                        switch (lowerTexts[selectIndex_LowerText.x])
                    {
                        case "CALL":
                            // Call
                            break;
                        case "RETRY":
                            // Retry
                            Debug.Log("M");
                            owner.pauseMenu = null;
                            owner.Kill();
                            break;
                        case "EXIT":
                            // Exit
                            //Application.Quit();
                            GameManager.GoToStageSelect();
                            break;
                        case "HELP":
                            // HELP
                            break;
                        case "ALT+F4":
                            Application.Quit();
                            break;
                    }
                    break;
            }
        }
    }
    /// <summary>
    /// The basic idea of all the Input functions is that they set their own index
    /// based on the input they receive and make sure it stays within bounds or
    /// switches to the appropriate segment every time.
    /// </summary>
    private void InputWeapon()
    {
        if (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);

            if (Input.GetAxisRaw("Vertical") > 0 && selectIndex_Weapon.y == 0)
            {
                mode = SelectModes.Players;
                return;
            }
            else if (Input.GetAxisRaw("Vertical") < 0 && selectIndex_Weapon.y * 2 + selectIndex_Weapon.x >= owner.weaponList.Count - 2)
            {
                if (selectIndex_Weapon.x == 0)
                    mode = SelectModes.RecoveryItems;
                else
                    mode = SelectModes.UtilityItems;
                return;
            }
            selectIndex_Weapon.y += Input.GetAxisRaw("Vertical") > 0 ? -1 : 1;
        }
        if (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            selectIndex_Weapon.x += Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
        }

        if (selectIndex_Weapon.y < 0)
            selectIndex_Weapon.y = 0;
        else if (selectIndex_Weapon.y > (owner.weaponList.Count + 1) / 2)
            selectIndex_Weapon.y = (owner.weaponList.Count + 1) / 2;

        if (selectIndex_Weapon.y - selectIndex_Weapon.z < 0)
            selectIndex_Weapon.z = selectIndex_Weapon.y;
        else if (selectIndex_Weapon.y - selectIndex_Weapon.z >= 6)
            selectIndex_Weapon.z = selectIndex_Weapon.y - 5;

        if (selectIndex_Weapon.x < 0)
            selectIndex_Weapon.x = 0;
        else if (selectIndex_Weapon.x > 1)
            selectIndex_Weapon.x = 1;

        if (selectIndex_Weapon.y * 2 + selectIndex_Weapon.x >= owner.weaponList.Count)
            selectIndex_Weapon.x = 0;
    }
    private void InputPlayer()
    {
        if (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                mode = SelectModes.Weapons;
                return;
            }
        }
        if (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            selectIndex_Player.x += Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
        }

        selectIndex_Player.x = Mathf.Clamp(selectIndex_Player.x, 0, playerData.Length - 1);
        if (selectIndex_Player.x < selectIndex_Player.z)
            selectIndex_Player.z = selectIndex_Player.x;
        else if (selectIndex_Player.z < selectIndex_Player.x - 3)
                 selectIndex_Player.z = selectIndex_Player.x - 3;
    }
    private void InputRecItems()
    {
        if (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            if (Input.GetAxisRaw("Vertical") > 0)
                mode = SelectModes.Weapons;
            else
                mode = SelectModes.LowerText;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
            return;
        }
        if (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            selectIndex_RecItems.x += Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
        }
        if (selectIndex_RecItems.x >= (int)GameManager.RecoveryItems.Length)
            mode = SelectModes.UtilityItems;

        selectIndex_RecItems.x = Mathf.Clamp(selectIndex_RecItems.x, 0, (int)GameManager.RecoveryItems.Length - 1);
        if (selectIndex_RecItems.x < selectIndex_RecItems.z)
            selectIndex_RecItems.z = selectIndex_RecItems.x;
        else if (selectIndex_RecItems.z < selectIndex_RecItems.x - 4)
                 selectIndex_RecItems.z = selectIndex_RecItems.x - 4;
    }
    private void InputUtItems()
    {
        if (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            if (Input.GetAxisRaw("Vertical") > 0)
                mode = SelectModes.Weapons;
            else
                mode = SelectModes.LowerText;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
            return;
        }
        if (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            selectIndex_UtItems.x += Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
        }

        if (selectIndex_UtItems.x < 0)
            mode = SelectModes.RecoveryItems;

        selectIndex_UtItems.x = Mathf.Clamp(selectIndex_UtItems.x, 0, (int)GameManager.UtilityItems.Length - 1);
        if (selectIndex_UtItems.x < selectIndex_UtItems.z)
            selectIndex_UtItems.z = selectIndex_UtItems.x;
        else if (selectIndex_UtItems.z < selectIndex_UtItems.x - 3)
                 selectIndex_UtItems.z = selectIndex_UtItems.x - 3;
    }
    private void InputLowerText()
    {
        if (Mathf.RoundToInt(Input.GetAxisRaw("Vertical")) != 0 && inputCooldown <= 0.0f)
        {
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                               Mathf.Round(Input.GetAxisRaw("Vertical")));
                inputCooldown = 0.25f;

                if (selectIndex_LowerText.x > 0)
                    mode = SelectModes.UtilityItems;
                else
                    mode = SelectModes.RecoveryItems;

                owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
                return;
            }
        }
        if (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")) != 0 && inputCooldown <= 0.0f)
        {
            lastInput = new Vector2(Mathf.Round(Input.GetAxisRaw("Horizontal")),
                           Mathf.Round(Input.GetAxisRaw("Vertical")));
            inputCooldown = 0.25f;

            selectIndex_LowerText.x += Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1;

            owner.audioStage.PlaySound(owner.SFXLibrary.menuMove, true);
        }

        selectIndex_LowerText.x = Mathf.Clamp(selectIndex_LowerText.x, 0, lowerTexts.Length - 1);
    }

    public override void Exit()
    {
        // Closes the pause menu.
        owner.pauseMenu = null;
        Time.timeScale = GameManager.globalTimeScale;
        owner.body.bodyType = RigidbodyType2D.Dynamic;
        owner.SetLocalTimeScale(Player.timeScale);
        owner.body.velocity = Vector2.zero;
        owner.body.simulated = true;
    }
    public IEnumerator Heal(float units, bool recoverHealth, bool recoverWeapons, GameManager.RecoveryItems recItem)
    {
        if (!recoverHealth && !recoverWeapons)
        {
            Debug.LogWarning("You aren't healing or recovering weapon energy. Possible oversight.");
            yield break;
        }

        // If there is no item, cancel.
        if (GameManager.recItemsOwned[(int)recItem] <= 0)
            yield break;

        // Recovers health and energy slowly.
        // Counts how many loops have happened.
        int loops = 0;
        while (units > 0)
        {
            // Checks if nothing was recovered the last frame to stop the loop.
            bool recoveredAnything = false;
            if (recoverHealth)
            {
                if (owner.health < owner.maxHealth)
                {
                    owner.health += Mathf.Min(units, 1);
                    owner.health = Mathf.Clamp(owner.health, 0, owner.maxHealth);
                    recoveredAnything = true;
                }
            }

            if (recoverWeapons)
            {
                foreach (Pl_WeaponData.Weapons wpnIndex in owner.weaponList)
                {
                    Pl_WeaponData wpn = Pl_WeaponData.WeaponList[(int)wpnIndex];

                    if (wpn.weaponEnergy < wpn.maxWeaponEnergy)
                    {
                        wpn.weaponEnergy += Mathf.Min(units, 1);
                        wpn.weaponEnergy = Mathf.Clamp(wpn.weaponEnergy, 0, wpn.maxWeaponEnergy);
                        recoveredAnything = true;
                    }
                }
            }

            // If there is no health or weapon energy recovered, there is no reason to use the item.
            // As it can't be known if everything is full beforehand without looping through everything,
            // and as it is done here anyway, we check within the final loop if anything has recovered.
            if (!recoveredAnything)
            {
                if (loops == 0)
                    yield break;
                break;
            }

            // Plays recovery sound and adds to variables.
            owner.audioStage.PlaySound(owner.SFXLibrary.healthRecover, true);

            units--;
            loops++;

            yield return new WaitForSecondsRealtime(0.05f);
        }

        // Consumes item.
        GameManager.recItemsOwned[(int)recItem] -= 1;

    }

    public override void DrawGUI()
    {
        // Draws everything.
        Vector2 cmrBase = new Vector2(Camera.main.rect.x * Screen.width, Camera.main.rect.y * Screen.height);
        int blockSize = (int)(Camera.main.pixelWidth / 16);
        GUI.contentColor = new Color32(56, 184, 248, 255);

        GUI.DrawTexture(new Rect(cmrBase.x, cmrBase.y, Camera.main.pixelWidth, Camera.main.pixelHeight), background);
        font.label.fontSize = (int)(blockSize * 0.5625f);

        // Draw the bolts
        Sprite bolt = (Sprite)Resources.LoadAll("Sprites/Stage/Items")[10];
        GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 12.825f * blockSize,
                                      cmrBase.y + 1.45f * blockSize,
                                      1.0f * blockSize,
                                      1.0f * blockSize),
                                      bolt.texture, new Rect(bolt.textureRect.x / bolt.texture.width, bolt.textureRect.y / bolt.texture.height,
                                      bolt.textureRect.width / bolt.texture.width, bolt.textureRect.height / bolt.texture.height));
        GUI.Label(new Rect(cmrBase.x + 14.3f * blockSize,
                                      cmrBase.y + 1.7f * blockSize,
                                      2.125f * blockSize,
                                      1.0f * blockSize),
                                      GameManager.bolts.ToString("0000"),
                                      font.label);


        // Draw the player section
        for (int i = selectIndex_Player.z; i < Mathf.Min(playerData.Length, selectIndex_Player.z + 4); i++)
        {
            Sprite icon = playerData[i].sprite;
            if (!icon)
                continue;

            int j = i - selectIndex_Player.z;

            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 4.0f * blockSize + j * 2.25f * blockSize,
                                                  cmrBase.y + 0.875f * blockSize + (selectIndex_Player.x == i && mode == SelectModes.Players ? -blockSize * 0.25f : 0),
                                                  1.5f * blockSize,
                                                  1.5f * blockSize),
                                                  icon.texture, new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                                                  icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height));
        }

        // Draw the weapon section
        for (int x = 0; x < Mathf.Min(owner.weaponList.Count, 2); x++)
        {
            for (int y = selectIndex_Weapon.z; y < Mathf.Min((owner.weaponList.Count + 2) / 2, selectIndex_Weapon.z + 6); y++)
            {

                if (y * 2 + x >= owner.weaponList.Count)
                    break;
                
                Pl_WeaponData weapon = Pl_WeaponData.WeaponList[(int)owner.weaponList[y * 2 + x]];

                if (weapon != null)
                {
                    Sprite icon = (selectIndex_Weapon.x == x && selectIndex_Weapon.y == y && mode == SelectModes.Weapons ? weapon.weaponIcon : weapon.weaponIconGray);
                    GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 3.0f * blockSize + x * 5.0f * blockSize,
                                 cmrBase.y + 3.5f * blockSize + (y - selectIndex_Weapon.z) * 1.0f * blockSize,
                                 1.0f * blockSize,
                                 1.0f * blockSize),
                                 icon.texture, new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                                 icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height));
                    GUI.Label(new Rect(cmrBase.x + 4.25f * blockSize + x * 5.0f * blockSize,
                                       cmrBase.y + 3.5f * blockSize + (y - selectIndex_Weapon.z) * 1.0f * blockSize,
                                       6.0f * blockSize,
                                       1.0f * blockSize),
                                       weapon.menuName,
                                       font.label);

                    for (int i = 0; i < weapon.maxWeaponEnergy; i++)
                    {
                        icon = (weapon.weaponEnergy >= i ? weapon.menuBarFull : weapon.menuBarEmpty);
                        GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 4.25f * blockSize + x * 5.0f * blockSize + i * 0.125f * blockSize,
                                                              cmrBase.y + 4.0f * blockSize + (y - selectIndex_Weapon.z) * 1.0f * blockSize,
                                                              0.125f * blockSize,
                                                              0.5f * blockSize),
                                                              icon.texture, new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                                                              icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height));
                    }
                }
            }
        }


        int prevSize = font.label.fontSize;
        font.label.fontSize = (int)(blockSize * 0.6f);
        // Draw the recovery item section
        for (int i = selectIndex_RecItems.z; i < Mathf.Min((int)GameManager.RecoveryItems.Length, selectIndex_RecItems.z + 5); i++)
        {
            Sprite icon = recItemSprites[i];
            int j = i - selectIndex_RecItems.z;

            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 3.0f * blockSize + j * 1.0f * blockSize,
                                                  cmrBase.y + 10.375f * blockSize + (selectIndex_RecItems.x == i && mode == SelectModes.RecoveryItems ? -blockSize * 0.25f : 0),
                                                  1.0f * blockSize,
                                                  1.0f * blockSize),
                                                  icon.texture, new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                                                  icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height));

            string num = GameManager.recItemsOwned[i].ToString("00");
            GUI.Label(new Rect(cmrBase.x + 3.125f * blockSize + j * 1.0f * blockSize,
                               cmrBase.y + 11.375f * blockSize,
                               2.0f * blockSize,
                               1.0f * blockSize),
                               num,
                               font.label);
        }

        // Draw the utility item section
        for (int i = 0; i < (int)GameManager.UtilityItems.Length; i++)
        {
            Sprite icon = utItemSprites[i];

            GUI.DrawTextureWithTexCoords(new Rect(cmrBase.x + 9.0f * blockSize + i * 1.0f * blockSize,
                                                  cmrBase.y + 10.375f * blockSize + (selectIndex_UtItems.x == i && mode == SelectModes.UtilityItems ? -blockSize * 0.25f : 0),
                                                  1.0f * blockSize,
                                                  1.0f * blockSize),
                                                  icon.texture, new Rect(icon.textureRect.x / icon.texture.width, icon.textureRect.y / icon.texture.height,
                                                  icon.textureRect.width / icon.texture.width, icon.textureRect.height / icon.texture.height));

            string num = GameManager.utItemsOwned[i].ToString("00");
            GUI.Label(new Rect(cmrBase.x + 9.125f * blockSize + i * 1.0f * blockSize,
                               cmrBase.y + 11.375f * blockSize,
                               2.0f * blockSize,
                               1.0f * blockSize),
                               num,
                               font.label);
        }

        // Draw the lower text section
        font.label.fontSize = (int)(blockSize * 0.7f);

        float prevLength = 0.0f;
        for (int i = 0; i < lowerTexts.Length; i++)
        {
            GUI.Label(new Rect(cmrBase.x + (3.0f + prevLength * 0.5f + 0.5f) * blockSize,
                               cmrBase.y + 12.25f * blockSize,
                               3.0f * blockSize,
                               1.0f * blockSize),
                               lowerTexts[i],
                               font.label);

            if (mode == SelectModes.LowerText && selectIndex_LowerText.x == i)
                GUI.Label(new Rect(cmrBase.x + (3.0f + prevLength * 0.5f + 0.125f) * blockSize,
                                   cmrBase.y + 12.25f * blockSize,
                                   3.0f * blockSize,
                                   1.0f * blockSize),
                                   ">",
                                   font.label);

            prevLength += lowerTexts[i].Length + 1.0f;
        }



        font.label.fontSize = prevSize;
    }



    public class PlayerData
    {
        public Sprite sprite;
        public GameManager.Players player;

        public PlayerData(GameManager.Players _player)
        {
            player = _player;
            sprite = GameManager.GetPlayerMenuSprite(_player);
        }
    }
}
