using UnityEngine;

/* The base item class. All items should derive from this. */

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{

    new public string name = "New Item";    // Name of the item
    public Sprite icon = null;              // Item icon
    public bool showInInventory = true;
    public bool withinChest = false;
    protected BattleMaster battleMaster; 

    // Called when the item is pressed in the inventory
    public virtual void Use()
    {
        // Use the item
    }

    public void RemoveFromInventory(Equipment equipment)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        if (battleMaster.battleStarted) //current character's inventory
        {
            battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(equipment);
            battleMaster.inventoryUI.UpdateUI();
        }
        else if (battleMaster.chestMenu.activeSelf) //chest menu is open
        {
            if (equipment.withinChest) //if clicking on an item in a chest
            {
                battleMaster.chest.items.Remove(equipment); //remove from chest
                battleMaster.chestContents.UpdateChestUI(); //update chest ui
                battleMaster.defaultCharacter.GetComponent<Inventory>().items.Add(equipment); //put in character's inventory
                battleMaster.chestInventoryUI.UpdateUI(); //update character's inventory ui (chest menu)
            }
            else //clicking on an item in the inventory
            {
                battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(equipment); //remove the item from the inventory
                battleMaster.chestInventoryUI.UpdateUI(); //update character inventory UI (chest menu)
            }
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
            if (item.withinChest) //if clicking on an item in a chest
            {
                battleMaster.chest.items.Remove(item); //remove from chest
                battleMaster.chestContents.UpdateChestUI(); //update chest ui
                battleMaster.defaultCharacter.GetComponent<Inventory>().items.Add(item); //put in character's inventory
                battleMaster.chestInventoryUI.UpdateUI(); //update character's inventory ui (chest menu)
            }
            else //clicking on an item in the inventory
            {
                battleMaster.defaultCharacter.GetComponent<Inventory>().items.Remove(item); //remove the item from the inventory
                battleMaster.chestInventoryUI.UpdateUI(); //update character inventory UI (chest menu)
            }
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