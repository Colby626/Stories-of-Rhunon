using UnityEngine;

/* The base item class. All items should derive from this. */

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{

    new public string name = "New Item";    // Name of the item
    public Sprite icon = null;              // Item icon
    public bool showInInventory = true;
    protected BattleMaster battleMaster; 

    // Called when the item is pressed in the inventory
    public virtual void Use()
    {
        // Use the item
    }

    public void MoveFromChestToInventory(Equipment equipment)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        battleMaster.chest.items.Remove(equipment); //remove from chest
        battleMaster.chestContents.UpdateChestUI(); //update chest ui
        battleMaster.defaultCharacter.GetComponent<Inventory>().items.Add(equipment); //put in character's inventory
        battleMaster.chestInventoryUI.UpdateUI(); //update character's inventory ui (chest menu)
    }

    public void MoveFromChestToInventory(Item item)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        battleMaster.chest.items.Remove(item); //remove from chest
        battleMaster.chestContents.UpdateChestUI(); //update chest ui
        battleMaster.defaultCharacter.GetComponent<Inventory>().items.Add(item); //put in character's inventory
        battleMaster.chestInventoryUI.UpdateUI(); //update character's inventory ui (chest menu)
    }

    public void RemoveFromInventory(Equipment equipment)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        if (battleMaster.battleStarted) //current character's inventory
        {
            battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(equipment);
            battleMaster.inventoryUI.UpdateUI();
        }
        else if (battleMaster.chestMenu.activeSelf)
        {
            battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(equipment);
            battleMaster.chestInventoryUI.UpdateUI();
        }
        else
        {
            battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(equipment);
            battleMaster.inventoryUI.UpdateUI();
        }
    }
    public void RemoveFromInventory(Item item)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        if (battleMaster.battleStarted)
        {
            battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(item);
            battleMaster.inventoryUI.UpdateUI();
        }
        else if (battleMaster.chestMenu.activeSelf)
        {
            battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(item);
            battleMaster.chestInventoryUI.UpdateUI();
        }
        else
        {
            battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(item);
            battleMaster.inventoryUI.UpdateUI();
        }
    }

    public void RemoveFromEquipment(EquipmentSlot slot)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        if (battleMaster.battleStarted)
        {
            battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[(int)slot] = null;
            battleMaster.equipmentManager.UpdateEquipmentUI();
        }
        else if (battleMaster.chestMenu.activeSelf)
        {
            battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment[(int)slot] = null;
            battleMaster.chestEquipmentManager.UpdateEquipmentUI();
        }
        else
        {
            battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment[(int)slot] = null;
            battleMaster.equipmentManager.UpdateEquipmentUI();
        }
    }
}