using UnityEngine;

/* The base item class. All items should derive from this. */

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{

    new public string name = "New Item";    // Name of the item
    public Sprite icon = null;              // Item icon
    public bool showInInventory = true;
    protected BattleMaster battleMaster;
    InventoryUI inventoryUI;
    EquipmentManager equipmentManager; 

    // Called when the item is pressed in the inventory
    public virtual void Use()
    {
        // Use the item
    }

    public void RemoveFromInventory(Equipment equipment)
    {
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(equipment);
        inventoryUI = GameObject.FindGameObjectWithTag("InventoryUI").GetComponent<InventoryUI>();
        inventoryUI.UpdateUI();
    }
    public void RemoveFromInventory(Item item)
    {
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(item);
        inventoryUI = GameObject.FindGameObjectWithTag("InventoryUI").GetComponent<InventoryUI>();
        inventoryUI.UpdateUI();
    }

    public void RemoveFromEquipment(EquipmentSlot slot)
    {
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[(int)slot] = null;
        equipmentManager = GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>();
        equipmentManager.UpdateEquipmentUI();
    }
}