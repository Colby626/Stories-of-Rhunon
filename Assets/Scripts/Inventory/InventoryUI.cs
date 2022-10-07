using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    Inventory inventory;
    InventorySlot[] slots;

    private void Start()
    {
        inventory = Inventory.instance;
        slots = inventory.GetComponentsInChildren<InventorySlot>(); 
    }

    void UpdateUI()
    {
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
}
