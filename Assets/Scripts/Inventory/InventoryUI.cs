using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [HideInInspector]
    public BattleMaster battleMaster;
    Inventory inventory;
    private void Start()
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
    }

    public void UpdateUI()
    {
        inventory = (battleMaster.battleStarted) ? battleMaster.currentCharacter.GetComponent<Inventory>() : battleMaster.defaultCharacter.GetComponent<Inventory>();

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