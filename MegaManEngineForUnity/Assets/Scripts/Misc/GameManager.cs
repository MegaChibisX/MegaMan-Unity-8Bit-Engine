using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GameManager script doesn't need to be in a scene to be used.
/// As it isn't a child of the "MonoBehaviour" script, you can't use functions like Start() or Update() with it.
/// We make the script "static", so we will only have one of it. It can't be Instantiated.
/// </summary>
public static class GameManager
{

    // Player related variables
    public static Vector3 playerPosition;

    public static Vector3 checkpointLocation;
    public static bool checkpointActive;
    public static Vector2 checkpointCamera_LeftCenter;
    public static float checkpointCamera_MaxRightMovement;
    public static float checkpointCamera_MaxUpMovement;
    public static bool roomFinishedLoading;

    public enum Players { MegaMan, ProtoMan, Bass, Roll, MegaManJet, MegaManPower, MegaManSuper, BassSuper }
    public static List<Players> availablePlayers;
    public enum RecoveryItems { ETank, WTank, MTank, LTank, RedBullTank, Yashichi, Length }
    public enum UtilityItems { Eddie, Beat, Tango, Reggae, Length }
    public static int[] recItemsOwned;
    public static int[] utItemsOwned;


    // Items owned
    public static bool item_DoubleGear;



    // Stage related variables
    public static float globalTimeScale = 1.0f;



    // Boss related variables
    public static int bossesActive;
    public static bool bossDead_PharaohMan;
    public static bool bossDead_GeminiMan;



    // Stage select variables
    public static Vector2Int lastStageSelected;
    public static int maxFortressStage;


    static GameManager()
    {
        // Everything here happens when the game starts, so it won't run
        // at the beginning of a room or a stage. Go to ResetRoom if something
        // needs to be reset every time a room restarts.
        playerPosition = Vector3.zero;

        checkpointLocation = playerPosition;
        checkpointActive = false;
        checkpointCamera_LeftCenter = Vector2.zero;
        checkpointCamera_MaxRightMovement = 0.0f;
        checkpointCamera_MaxUpMovement = 0.0f;

        availablePlayers = new List<Players>();
        availablePlayers.Add(Players.MegaMan);
        availablePlayers.Add(Players.ProtoMan);
        availablePlayers.Add(Players.Bass);
        availablePlayers.Add(Players.Roll);
        availablePlayers.Add(Players.MegaManJet);
        availablePlayers.Add(Players.MegaManPower);
        availablePlayers.Add(Players.MegaManSuper);
        availablePlayers.Add(Players.BassSuper);

        recItemsOwned = new int[(int)RecoveryItems.Length];
        for (int i = 0; i < recItemsOwned.Length; i++)
            recItemsOwned[i] = Random.Range(0, 11);
        utItemsOwned = new int[(int)UtilityItems.Length];
        for (int i = 0; i < utItemsOwned.Length; i++)
            utItemsOwned[i] = Random.Range(0, 11);

        item_DoubleGear = true;

        globalTimeScale = 1.0f;

        bossesActive = 0;
        bossDead_PharaohMan = false;
        bossDead_GeminiMan = false;

        lastStageSelected = Vector2Int.one;
        maxFortressStage = 3;
    }

    public static void ResetRoom()
    {
        // Resets the room.
        bossesActive = 0;
        roomFinishedLoading = false;


        foreach (Pl_WeaponData wpn in Pl_WeaponData.WeaponList)
        {
            wpn.Init();
        }
    }

    public static Sprite GetPlayerMenuSprite(Players pl)
    {
        // Gets the appropriate Character Sprite from the Resources.
        switch (pl)
        {
            default:
            case Players.MegaMan:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[0];
            case Players.ProtoMan:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[1];
            case Players.Bass:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[2];
            case Players.Roll:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[3];
            case Players.MegaManJet:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[4];
            case Players.MegaManPower:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[5];
            case Players.MegaManSuper:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[6];
            case Players.BassSuper:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[7];

        }
    }
	


}
