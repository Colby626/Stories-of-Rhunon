using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    Item item;

    public Image icon;

    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = newItem.icon;

        item.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        item.icon = null;
        item.enabled = false;
    }
}
