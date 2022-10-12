using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public BattleMaster battleMaster;
    Inventory inventory;

    public void UpdateUI()
    {
        inventory = battleMaster.currentCharacter.GetComponent<Inventory>();

        InventorySlot[] slots = GetComponentsInChildren<InventorySlot>(); 

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void ClearUI()
    {
        InventorySlot[] slots = GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }
}
