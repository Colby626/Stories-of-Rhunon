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

    public void Unequip()
    {
        if (item != null)
        {
            BattleMaster battleMaster = transform.parent.GetComponent<EquipmentManager>().battleMaster;
            if (battleMaster.chestMenu.activeSelf)
            {
                battleMaster.chestEquipmentManager.Unequip((Equipment)item);
            }
            else
            {
                battleMaster.equipmentManager.Unequip((Equipment)item);
            }
            
        }
    }
}
