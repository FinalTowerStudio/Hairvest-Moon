using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldItemPickup : MonoBehaviour
    {
        public ItemData itemData;
        public int quantity = 1;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyItemVisual();
        }

        private void ApplyItemVisual()
        {
            if (itemData != null)
            {
                spriteRenderer.sprite = itemData.itemIcon;
            }
            else
            {
                Debug.LogWarning("ItemData not assigned on WorldItemPickup.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                bool added = false;

                if (itemData.itemType == ItemType.Seed || itemData.itemType == ItemType.Crop)
                {
                    added = ServiceLocator.Get<ResourceInventorySystem>().AddItem(itemData, quantity);
                }
                else
                {
                    if (ServiceLocator.Get<BackpackInventorySystem>().CanAddItem(itemData, quantity))
                    {
                        added = ServiceLocator.Get<BackpackInventorySystem>().AddItem(itemData, quantity);
                    }
                    else
                    {
                        Debug.Log("Cannot pick up item: Backpack full.");
                        // (Optional) Show some floating feedback message to player.
                    }
                }

                if (added)
                {
                    Debug.Log($"Picked up {quantity}x {itemData.itemName}");
                    Destroy(gameObject);
                }
            }
        }


    }
}