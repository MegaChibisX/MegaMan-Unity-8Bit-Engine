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

    public enum Players { MegaMan, ProtoMan, Bass, Roll, MegaManJet, MegaManPower, MegaManSuper, BassSuper, X }
    public static List<Players> availablePlayers;
    public enum RecoveryItems { ETank, WTank, MTank, LTank, RedBullTank, Yashichi, Length }
    public enum UtilityItems { Eddie, Beat, Tango, Reggae, Length }
    public static int[] recItemsOwned;
    public static int[] utItemsOwned;


    // Items owned
    public static int bolts;
    public static bool item_DoubleGear;

    // The items found in the current stage and whether they are picked up or not.
    public static Dictionary<int, bool> stageItems;



    // Stage related variables
    public static float globalTimeScale = 1.0f;
    public static bool preventDeathReset = false;


    // Boss related variables
    public static int bossesActive;
    public static bool bossDead_PharaohMan;
    public static bool bossDead_GeminiMan;
    public static bool bossDead_MetalMan;
    public static bool bossDead_StarMan;
    public static bool bossDead_BombMan;
    public static bool bossDead_WindMan;
    public static bool bossDead_GalaxyMan;
    public static bool bossDead_CommandoMan;



    // Stage select variables
    public static Vector2Int lastStageSelected;
    public static int maxFortressStage;
    public static bool playFortressStageUnlockAnimation;
    private static int prevFortressStage;
    private static int prevBossesDead;


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
        availablePlayers.Add(Players.X);

        recItemsOwned = new int[(int)RecoveryItems.Length];
        for (int i = 0; i < recItemsOwned.Length; i++)
            recItemsOwned[i] = 0;
        utItemsOwned = new int[(int)UtilityItems.Length];
        for (int i = 0; i < utItemsOwned.Length; i++)
            utItemsOwned[i] = 0;

        bolts = 0;
        item_DoubleGear = true;
        stageItems = new Dictionary<int, bool>();

        globalTimeScale = 1.0f;
        preventDeathReset = false;

        bossesActive = 0;
        bossDead_PharaohMan  = false;
        bossDead_GeminiMan   = false;
        bossDead_MetalMan    = false;
        bossDead_StarMan     = false;
        bossDead_BombMan     = false;
        bossDead_WindMan     = false;
        bossDead_GalaxyMan   = false;
        bossDead_CommandoMan = false;

        lastStageSelected = Vector2Int.one;
        maxFortressStage = 0;
        prevFortressStage = maxFortressStage;
        playFortressStageUnlockAnimation = false;

    }

    public static void ResetRoom()
    {
        // Resets the room.
        bossesActive = 0;
        roomFinishedLoading = false;
        Time.timeScale = 1.0f;
        preventDeathReset = false;

        foreach (Item_Pickup item in Object.FindObjectsOfType<Item_Pickup>())
        {
            int hashCode = new Vector2(item.transform.position.x, item.transform.position.z).GetHashCode();
            if (stageItems.ContainsKey(hashCode) && stageItems[hashCode] == false)
                Object.Destroy(item.gameObject);
            if (stageItems.ContainsKey(hashCode))
                Debug.Log("Contains! " + stageItems[hashCode]);
        }
    }
    public static void StartRoom()
    {
        ResetRoom();
        bossesActive = 0;
        
        //
        stageItems.Clear();
        foreach (Item_Pickup item in Object.FindObjectsOfType<Item_Pickup>())
        {
            int hashCode = new Vector2(item.transform.position.x, item.transform.position.z).GetHashCode();
            if (!stageItems.ContainsKey(hashCode))
                stageItems.Add(hashCode, true);
            else
                Object.Destroy(item.gameObject);
        }

        foreach (Pl_WeaponData wpn in Pl_WeaponData.WeaponList)
        {
            if (wpn != null) 
                wpn.Init();
        }
    }

    public static void GoToStageSelect()
    {
        int bossesKilled = 0;
        if (bossDead_BombMan)
            bossesKilled++;
        if (bossDead_CommandoMan)
            bossesKilled++;
        if (bossDead_GalaxyMan)
            bossesKilled++;
        if (bossDead_GeminiMan)
            bossesKilled++;
        if (bossDead_MetalMan)
            bossesKilled++;
        if (bossDead_PharaohMan)
            bossesKilled++;
        if (bossDead_StarMan)
            bossesKilled++;
        if (bossDead_WindMan)
            bossesKilled++;

        if (prevBossesDead != bossesKilled)
        {
            prevBossesDead = bossesKilled;
            if (bossesKilled == 4)
                Helper.GoToStage("Cutscene_Halfway");
            else
            {
                if (bossesKilled == 8 && prevFortressStage == 0)
                {
                    playFortressStageUnlockAnimation = true;
                    maxFortressStage = 1;
                    prevFortressStage = maxFortressStage;
                    Helper.GoToStage("Cutscene_PreFortress");
                }
                else
                {
                    prevFortressStage = maxFortressStage;
                    Helper.GoToStage("StageSelect");
                }
            }
        }
        else
        {
            prevBossesDead = bossesKilled;
            prevFortressStage = maxFortressStage;
            Helper.GoToStage("StageSelect");
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
            case Players.X:
                return Resources.LoadAll<Sprite>("Sprites/Menus/Characters")[8];

        }
    }
    public static GameObject GetPlayerPrefabPath(Players pl)
    {
        // Gets the appropriate Character Sprite from the Resources.
        switch (pl)
        {
            default:
            case Players.MegaMan:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/MegaMan");
            case Players.ProtoMan:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/ProtoMan");
            case Players.MegaManJet:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/MegaMan_Jet");
            case Players.MegaManPower:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/MegaMan_Power");
            case Players.Bass:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/Bass");
            case Players.X:
                return (GameObject)Resources.Load<GameObject>("Prefabs/Players/X");


        }
    }
    public static string GetPlayerSpritePath(Players pl)
    {
        switch (pl)
        {
            default:
            case Players.MegaMan:
                return "Sprites/Players/MegaMan/MegaMan_Full";
            case Players.ProtoMan:
                return "Sprites/Players/ProtoMan/ProtoMan_Full";
            case Players.MegaManJet:
                return "Sprites/Players/MegaMan_Jet/MegaMan_Full";
            case Players.MegaManPower:
                return "Sprites/Players/MegaMan_Power/Power_Full";
            case Players.Bass:
                return "Sprites/Players/Bass/Bass_Full";
            case Players.X:
                return "Sprites/Players/Bass/X_Full";


        }
    }

    public static void ShakeCamera(float time, float strength, bool dropPlayer)
    {
        if (CameraCtrl.instance != null)
            CameraCtrl.instance.Shake(time, strength, dropPlayer);
    }

    /// <summary>
    /// This saves the current game data. Simplified to be more intuitive.
    /// Writing an external file and reading from it is really common when it comes to coding,
    /// so you can find plenty of info online if you've got questions.
    /// They can also be edited by the player with a text editor as they are right now.
    /// </summary>
    /// <returns></returns>
    public static string SaveData(int position)
    {
        //  Writes the data as a string.
        string data = "";
        data += bossDead_BombMan ? "1" : "0";
        data += bossDead_MetalMan ? "1" : "0";
        data += bossDead_GeminiMan ? "1" : "0";
        data += bossDead_PharaohMan ? "1" : "0";
        data += bossDead_StarMan ? "1" : "0";
        data += bossDead_WindMan ? "1" : "0";
        data += bossDead_GalaxyMan ? "1" : "0";
        data += bossDead_CommandoMan ? "1" : "0";

        data += maxFortressStage.ToString("00");

        data += recItemsOwned[(int)RecoveryItems.ETank].ToString("00");
        data += recItemsOwned[(int)RecoveryItems.WTank].ToString("00");
        data += recItemsOwned[(int)RecoveryItems.MTank].ToString("00");
        data += recItemsOwned[(int)RecoveryItems.LTank].ToString("00");
        data += recItemsOwned[(int)RecoveryItems.RedBullTank].ToString("00");
        data += recItemsOwned[(int)RecoveryItems.Yashichi].ToString("00");

        data += bolts.ToString("0000");

        // Creates a new file.
        string path = Application.dataPath + "/GameSaveData/";
        string fileName = "SaveData_" + position.ToString("00");
        Debug.Log(path);

        // Creates a folder, if one doesn't already exist.
        System.IO.Directory.CreateDirectory(path);

        System.IO.StreamWriter sw = System.IO.File.CreateText(path + fileName);
        using (sw)
        {
            sw.Write(data);
        }
        sw.Close();

        return data;
    }
    public static string LoadData(int position,  bool load)
    {
        string path = Application.dataPath + "/GameSaveData/";
        string fileName = "SaveData_" + position.ToString("00");
        Debug.Log(path);

        if (!System.IO.File.Exists(path + fileName))
            return null;

        System.IO.StreamReader sr = System.IO.File.OpenText(path + fileName);

        string s = "";
        using (sr)
        {
            if ((s = sr.ReadLine()) == null)
            {
                sr.Close();
                return null;
            }

            if (!load)
            {
                sr.Close();
                return s;
            }

            int output = 0;
            int.TryParse(s[0].ToString(), out output);
            bossDead_BombMan = output == 1;
            int.TryParse(s[1].ToString(), out output);
            bossDead_MetalMan = output == 1;
            int.TryParse(s[2].ToString(), out output);
            bossDead_GeminiMan = output == 1;
            int.TryParse(s[3].ToString(), out output);
            bossDead_PharaohMan = output == 1;
            int.TryParse(s[4].ToString(), out output);
            bossDead_StarMan = output == 1;
            int.TryParse(s[5].ToString(), out output);
            bossDead_WindMan = output == 1;
            int.TryParse(s[6].ToString(), out output);
            bossDead_GalaxyMan = output == 1;
            int.TryParse(s[7].ToString(), out output);
            bossDead_CommandoMan = output == 1;
            int.TryParse(s.Substring(8, 2), out output);
            maxFortressStage = output;
            int.TryParse(s.Substring(10, 2), out output);
            recItemsOwned[(int)RecoveryItems.ETank]= output;
            int.TryParse(s.Substring(12, 2), out output);
            recItemsOwned[(int)RecoveryItems.WTank] = output;
            int.TryParse(s.Substring(14, 2), out output);
            recItemsOwned[(int)RecoveryItems.MTank] = output;
            int.TryParse(s.Substring(16, 2), out output);
            recItemsOwned[(int)RecoveryItems.LTank] = output;
            int.TryParse(s.Substring(18, 2), out output);
            recItemsOwned[(int)RecoveryItems.RedBullTank] = output;
            int.TryParse(s.Substring(20, 2), out output);
            recItemsOwned[(int)RecoveryItems.Yashichi] = output;

            int.TryParse(s.Substring(22, 4), out output);
            bolts = output;

        }

        sr.Close();
        return s;
    }

}
