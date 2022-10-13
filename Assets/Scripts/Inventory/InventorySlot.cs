using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image icon;

    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = newItem.icon;

        icon.enabled = true;
    }
    public void AddEquipment(Equipment newEquipment)
    {
        item = newEquipment;

        icon.enabled = true;

        icon.sprite = newEquipment.icon;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
    }

    // Use the item
    public void UseItem()
    {
        if (item != null)
        {
            item.Use();
        }
    }
}
