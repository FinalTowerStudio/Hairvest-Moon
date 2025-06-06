using HairvestMoon.Core;
using HairvestMoon.Inventory;
using HairvestMoon.UI;
using HairvestMoon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemData item;
    private InstallConfirmUI installConfirmUI;

    private void Start()
    {
        installConfirmUI = ServiceLocator.Get<InstallConfirmUI>();
    }

    public void Initialize(ItemData itemData)
    {
        item = itemData;
    }

    public void UpdateDisplay()
    {
        bool discovered = ServiceLocator.Get<ResourceInventorySystem>().discoveredItems.Contains(item);
        iconImage.sprite = discovered ? item.itemIcon : null;
        iconImage.color = discovered ? Color.white : Color.gray;
        nameText.text = discovered ? item.itemName : "????";

        int quantity = ServiceLocator.Get<ResourceInventorySystem>().GetQuantity(item);
        quantityText.text = discovered ? quantity.ToString() : "";
    }

    public void OnClick()
    {
        if (item.itemType == ItemType.Tool || item.itemType == ItemType.Upgrade)
        {
            installConfirmUI.Show(item);
        }
    }
}
