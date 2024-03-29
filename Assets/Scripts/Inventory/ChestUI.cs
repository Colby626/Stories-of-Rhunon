using UnityEngine;

public class ChestUI : MonoBehaviour
{
    private BattleMaster battleMaster;
    private Inventory inventory;

    public void UpdateChestUI()
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        inventory = battleMaster.chest;

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

    public void ClearChestUI()
    {
        InventorySlot[] slots = GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearSlot();
        }
    }
}