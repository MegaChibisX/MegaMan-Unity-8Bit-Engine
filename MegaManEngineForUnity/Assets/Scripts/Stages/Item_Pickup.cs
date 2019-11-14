using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the in-game item class, for items that mostly
/// drop from enemies. Each item needs to be matched with
/// an Item.Items, which tells the item how much health/energy/gear to recover.
/// 
/// For more information about how the items function, check Item.cs.
/// </summary>
[AddComponentMenu("MegaMan/Item/Stage Item (Item_Pickup)")]
public class Item_Pickup : MonoBehaviour {

    // The item whose information will be used when picked up by the player.
    public Item.Items itemToGive;
    private Item item;

    // Used for collectible items.
    public bool addToInventory = false;

    // The frames of the animation, as Items don't have very complex animations.
    public Sprite[] animationSprites;
    public AudioClip itemSound;

    // The Sprite Renderer, as it needs to be changed by the script.
    private SpriteRenderer rend;
    // Checks if the item has already been picked, to precent multiple colliders
    // from all registering a contact with the item.
    private bool picked = false;


    private void Start()
    {
        // Error checks.
        rend = GetComponentInChildren<SpriteRenderer>();
        if (rend == null)
            Debug.LogWarning("There is no Sprite Renderer in Item Pickup " + name + "!");
        if (Item.itemList.Length <= (int)itemToGive)
        {
            Debug.LogError("There is no item in the Item List that matches the Enum value " + itemToGive + "!\nAre you sure the ItemList and Items enum match in the Item.cs class?");
            Destroy(gameObject);
        }
        item = Item.itemList[(int)itemToGive];
    }

    private void LateUpdate()
    {
        if (animationSprites == null || animationSprites.Length == 0)
            return;

        // Sets the appropriate animation frame.
        int maxFrame = animationSprites.Length;
        int frame = (int)((Time.unscaledTime % (maxFrame / 3f)) * maxFrame);

        rend.sprite = animationSprites[frame];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the item is picked by a player, it registers as a pickup.
        Player player = null;
        if (!picked && collision.attachedRigidbody != null && (player = collision.attachedRigidbody.GetComponent<Player>()) != null)
        {
            picked = true;
            if (addToInventory)
            {
                // If the item should be added to the inventory, and the item can be in the inventory, it is added.
                switch (item.itemType)
                {
                    case Item.Items.ETank:
                        GameManager.recItemsOwned[(int)GameManager.RecoveryItems.ETank]++;
                        break;
                    case Item.Items.WTank:
                        GameManager.recItemsOwned[(int)GameManager.RecoveryItems.WTank]++;
                        break;
                    case Item.Items.MTank:
                        GameManager.recItemsOwned[(int)GameManager.RecoveryItems.MTank]++;
                        break;
                    case Item.Items.LTank:
                        GameManager.recItemsOwned[(int)GameManager.RecoveryItems.LTank]++;
                        break;
                    case Item.Items.boltSmall:
                        GameManager.bolts += 1;
                        break;
                    case Item.Items.boltBig:
                        GameManager.bolts += 5;
                        break;
                    case Item.Items.boltHuge:
                        GameManager.bolts += 100;
                        break;
                }
                Helper.PlaySound(itemSound);

                int hashCode = new Vector2(transform.position.x, transform.position.z).GetHashCode();
                if (GameManager.stageItems.ContainsKey(hashCode))
                {
                    print(hashCode + " found!");
                    GameManager.stageItems[hashCode] = false;
                }
                else
                    print(hashCode + " failed!");
            }
            else
            {
                // If the item should be used immediately, it gets used.
                player.StartCoroutine(item.Use(player, false, itemSound));
            }
            switch (item.itemType)
            {
                // Handles special items here.
                case Item.Items.DoubleGearChip:
                    player.gearAvailable = true;
                    break;
            }
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GetComponent<Rigidbody2D>())
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, GetComponent<Rigidbody2D>().velocity.y);
    }


}
