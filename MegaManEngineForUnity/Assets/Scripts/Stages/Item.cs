using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the class that keeps track of all the Items
/// and what each one does. Most of the items are fairly
/// similar, just recovering an amount of health, weapon
/// energy and gear. For that reason, there is one big list
/// that keeps track of what every item does.
/// 
/// This is not the actual in-game item class that is used
/// by items dropped by enemies. For these, check Item_Pickup.cs.
/// </summary>
public class Item
{

    // This is the list that keeps track of all the items that can exist,
    // and registers how much health, weapon energy, gear every item recovers,
    // as well as their bolt cost from a store. Bolts have negative costs
    // as they are how you gain money.
    // Make sure that every item is listed in the 'enum Items' and that
    // it is registered in the itemList, with its position in the enum
    // matching its position in the list. Both of these are static,
    // as each item doesn't need its own list, and the list needs
    // to be accessed by outside scripts without a specific item
    // object being present at a specific point in time.
    public enum Items { empty, smallHealth, bigHealth, smallEnergy, bigEnergy, smallGear, bigGear, boltSmall, boltBig, boltHuge,
                        OneUp, ETank, WTank, MTank, LTank, DoubleGearChip }

    public static Item[] itemList =
    {
        new Item(0, 0, 0, 0, Items.empty),
        new Item(2, 0, 0, 2, Items.smallHealth),
        new Item(5, 0, 0, 5, Items.bigHealth),
        new Item(0, 2, 0, 2, Items.smallEnergy),
        new Item(0, 5, 0, 5, Items.bigEnergy),
        new Item(0, 0, 7, 2, Items.smallGear),
        new Item(0, 0, 28, 5, Items.bigGear),
        new Item(0, 0, 0, -1, Items.boltSmall),
        new Item(0, 0, 0, -5, Items.boltBig),
        new Item(0, 0, 0, -100, Items.boltHuge),
        new Item(28, 0, 28, 50, Items.OneUp),
        new Item(28, 0, 0, 100, Items.ETank),
        new Item(0, 28, 0, 50, Items.WTank),
        new Item(28, 1000, 0, 300, Items.MTank),
        new Item(28, 1000, 0, 300, Items.LTank),
        new Item(0, 0, 0, 0, Items.DoubleGearChip)
    };

    // This is the type of the current item.
    public Items itemType;

    // Speaks for itself.
    public float addHealth = 0.0f;
    public float addEnergy = 0.0f;
    public float addGear = 0.0f;

    public int boltCost = 0;

    public Item(float _addHealth, float _addEnergy, float _addGear, int _boltCost, Items _itemType)
    {
        itemType = _itemType;
        addHealth = _addHealth;
        addEnergy = _addEnergy;
        addGear = _addGear;
        boltCost = _boltCost;
    }


    public IEnumerator Use(Player player, bool instant, AudioClip itemSound)
    {
        // Use() is called when a player uses the item from the Pause Menu.
        if (instant)
        {
            // Instantly recovers health, making sure it stays within the allowed bounds.
            player.health = Mathf.Clamp(player.health + addHealth, 0, 28);
            // Instantly recovers the same amount of weapon energy from all weapons,
            // making sure it all stays within the allowed bounds.
            if (player.currentWeapon != null)
                player.currentWeapon.weaponEnergy = Mathf.Clamp(player.currentWeapon.weaponEnergy + addEnergy, 0, 28);
            // Instantly recovers gear energy, making sure it stays within the allowed bounds.
            player.gearGauge = Mathf.Clamp(player.gearGauge + addGear, 0, 28);
        }
        else
        {
            // If there is a delay, everything needs to stop for a while.
            Time.timeScale = 0.0f;

            // The 'thingRemains' counts how much health/energy/gear is left that
            // the player can recover. When there's nothing left to recover, the game continues.
            float healthRemains = addHealth;
            if (player.health >= 28.0f)
                healthRemains = 0.0f;
            else
                healthRemains = Mathf.Min(healthRemains, 28.0f - player.health);

            float energyRemains = addEnergy;
            if (player.currentWeapon == null || player.currentWeapon.weaponEnergy >= player.currentWeapon.maxWeaponEnergy)
                energyRemains = 0.0f;
            else
                energyRemains = Mathf.Min(energyRemains, 28.0f - player.currentWeapon.weaponEnergy);


            float gearRemains = addGear;
            if (!player.gearAvailable || player.gearGauge >= 28)
                gearRemains = 0.0f;
            else
                gearRemains = Mathf.Min(gearRemains, 28.0f - player.gearGauge);



            // Adds 1 unit of everything, or as much remains, in case of non-integers.
            while (healthRemains > 0 || energyRemains > 0|| gearRemains > 0)
            {
                if (healthRemains > 0.0f)
                {
                    player.health += Mathf.Min(healthRemains, 1.0f);
                    healthRemains--;
                    if (player.health == 28)
                        healthRemains = 0.0f;
                }
                if (energyRemains > 0.0f)
                {
                    player.currentWeapon.weaponEnergy += Mathf.Min(energyRemains, 1.0f);
                    energyRemains--;
                    if (player.currentWeapon.weaponEnergy == 28)
                        addEnergy = 0.0f;
                }
                if (gearRemains > 0.0f)
                {
                    player.gearGauge += Mathf.Min(gearRemains, 1.0f);
                    gearRemains--;
                    if (player.gearGauge == 28)
                        addGear = 0.0f;
                }

                // Plays the recovery sound and waits the appropriate time.
                Helper.PlaySound(itemSound);

                yield return new WaitForSecondsRealtime(0.075f);
            }

            // Resets the time.
            Time.timeScale = GameManager.globalTimeScale;
        }
    }


    public static Items GetRandomItem(float multiNone, float multiSmHp, float multiBgHp, float multiSmEn, float multiBgEn, float multiSmGr, float multiBgGr,
                                       float multi1Up)
    {
        // Random drops are handled here the following way. A list is made with what items can
        // be dropped and what the chance of every item being dropped is. A KeyValuePair is
        // used to keep track of every item and the chance it will drop, as well as the chance
        // of nothing dropping. All of the chances have multipliers that can be given as parameters.
        //
        // This is a bit more technical, and there are probably better ways to do random drops.
        List<KeyValuePair<Items, float>> chances = new List<KeyValuePair<Items, float>>();
        chances.Add(new KeyValuePair<Items, float>(Items.empty,      5.0f * multiNone));
        chances.Add(new KeyValuePair<Items, float>(Items.smallHealth,       multiSmHp));
        chances.Add(new KeyValuePair<Items, float>(Items.bigHealth,         multiBgHp));
        chances.Add(new KeyValuePair<Items, float>(Items.smallEnergy,       multiSmEn));
        chances.Add(new KeyValuePair<Items, float>(Items.bigEnergy,         multiBgEn));
        chances.Add(new KeyValuePair<Items, float>(Items.smallGear,  0.7f * multiSmGr));
        chances.Add(new KeyValuePair<Items, float>(Items.bigGear,    0.7f * multiBgGr));
        chances.Add(new KeyValuePair<Items, float>(Items.OneUp,      0.2f * multi1Up));

        // Gets a random number inside the list and returns the item that matches it.
        float totalChances = 0;
        foreach (KeyValuePair<Items, float> pair in chances)
            totalChances += pair.Value;

        float itemChoice = Random.Range(0, totalChances);
        for (int i = 0; i < chances.Count; i++)
        {
            itemChoice -= chances[i].Value;
            if (itemChoice <= 0)
                return chances[i].Key;
        }
        return Items.empty;
    }
    public static Items GetRandomBolt(float multiNone, float multiSmall, float multiBig, float multiHuge)
    {
        // Random drops are handled here the following way. A list is made with what items can
        // be dropped and what the chance of every item being dropped is. A KeyValuePair is
        // used to keep track of every item and the chance it will drop, as well as the chance
        // of nothing dropping. All of the chances have multipliers that can be given as parameters.
        //
        // This is a bit more technical, and there are probably better ways to do random drops.
        List<KeyValuePair<Items, float>> chances = new List<KeyValuePair<Items, float>>();
        chances.Add(new KeyValuePair<Items, float>(Items.empty, 5.0f * multiNone));
        chances.Add(new KeyValuePair<Items, float>(Items.boltSmall, 3 * multiSmall));
        chances.Add(new KeyValuePair<Items, float>(Items.boltBig, multiBig));
        chances.Add(new KeyValuePair<Items, float>(Items.boltHuge, 0.02f * multiHuge));

        // Gets a random number inside the list and returns the item that matches it.
        float totalChances = 0;
        foreach (KeyValuePair<Items, float> pair in chances)
            totalChances += pair.Value;

        float itemChoice = Random.Range(0, totalChances);
        for (int i = 0; i < chances.Count; i++)
        {
            itemChoice -= chances[i].Value;
            if (itemChoice <= 0)
                return chances[i].Key;
        }
        return Items.empty;
    }
    public static GameObject GetObjectFromItem(Items item)
    {
        // Returns the appropriate Item GameObject based on the Enum Item.
        switch (item)
        {
            default:
            case Items.empty:
                return null;
            case Items.smallHealth:
                return (GameObject)Resources.Load("Prefabs/Items/HealthSmall", typeof(GameObject));
            case Items.bigHealth:
                return (GameObject)Resources.Load("Prefabs/Items/HealthBig", typeof(GameObject));
            case Items.smallEnergy:
                return (GameObject)Resources.Load("Prefabs/Items/WeaponSmall", typeof(GameObject));
            case Items.bigEnergy:
                return (GameObject)Resources.Load("Prefabs/Items/WeaponBig", typeof(GameObject));
            case Items.smallGear:
                return (GameObject)Resources.Load("Prefabs/Items/GearSmall", typeof(GameObject));
            case Items.bigGear:
                return (GameObject)Resources.Load("Prefabs/Items/GearBig", typeof(GameObject));
            case Items.boltSmall:
                return (GameObject)Resources.Load("Prefabs/Items/BoltSmall", typeof(GameObject));
            case Items.boltBig:
                return (GameObject)Resources.Load("Prefabs/Items/BoltBig", typeof(GameObject));
            case Items.boltHuge:
                return (GameObject)Resources.Load("Prefabs/Items/BoltHuge", typeof(GameObject));
            case Items.OneUp:
                return (GameObject)Resources.Load("Prefabs/Items/OneUp", typeof(GameObject));
            case Items.ETank:
                return (GameObject)Resources.Load("Prefabs/Items/Tank-E", typeof(GameObject));
            case Items.WTank:
                return (GameObject)Resources.Load("Prefabs/Items/Tank-W", typeof(GameObject));
            case Items.MTank:
                return (GameObject)Resources.Load("Prefabs/Items/Tank-M", typeof(GameObject));
            case Items.LTank:
                return (GameObject)Resources.Load("Prefabs/Items/Tank-L", typeof(GameObject));
            case Items.DoubleGearChip:
                return (GameObject)Resources.Load("Prefabs/Items/Double Gear Chip", typeof(GameObject));
        }
    }

    public static int GetItemQuantity(Items item)
    {
        switch (item)
        {
            case Items.ETank:
                return GameManager.recItemsOwned[(int)GameManager.RecoveryItems.ETank];
            case Items.WTank:
                return GameManager.recItemsOwned[(int)GameManager.RecoveryItems.WTank];
            case Items.MTank:
                return GameManager.recItemsOwned[(int)GameManager.RecoveryItems.MTank];
            case Items.LTank:
                return GameManager.recItemsOwned[(int)GameManager.RecoveryItems.LTank];
            default:
                return 0;
        }
    }
    public static bool AddItemQuantity(Items item, int  number)
    {
        switch (item)
        {
            case Items.ETank:
                GameManager.recItemsOwned[(int)GameManager.RecoveryItems.ETank] += number;
                return true;
            case Items.WTank:
                GameManager.recItemsOwned[(int)GameManager.RecoveryItems.WTank] += number;
                return true;
            case Items.MTank:
                GameManager.recItemsOwned[(int)GameManager.RecoveryItems.MTank] += number;
                return true;
            case Items.LTank:
                GameManager.recItemsOwned[(int)GameManager.RecoveryItems.LTank] += number;
                return true;
            case Items.boltSmall:
                GameManager.bolts += 1;
                return true;
            case Items.boltBig:
                GameManager.bolts += 5;
                return true;
            case Items.boltHuge:
                GameManager.bolts += 100;
                return true;
            default:
                return false;
        }
    }


}
