using UnityEngine;

// An Item that can be equipped to increase armor/damage

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Equipment")]
public class Equipment : Item
{
    public EquipmentSlot equipSlot;     // What slot to equip it in
    public int damageNegation;
    public int damageIncrease;

    // Called when pressed in the inventory
    public override void Use()
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        if (battleMaster.battleStarted)
        {
            if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment.Contains(this))
            {
                battleMaster.equipmentManager.Unequip(this);  //Unequip
                RemoveFromEquipment(equipSlot); //Remove from equipment
            }
            else
            {
                battleMaster.equipmentManager.Equip(this);  // Equip
                RemoveFromInventory(this);  //Remove from inventory
            }
        }
        else if (battleMaster.chestMenu.activeSelf)
        {
            if (withinChest) //if this item is within a chest when clicked on
            {
                Debug.Log("clicking on an item in a chest");
                battleMaster.chest.items.Remove(this);
                battleMaster.chestContents.UpdateChestUI();
                battleMaster.defaultCharacter.GetComponent<Inventory>().Add(this);
                battleMaster.chestInventoryUI.UpdateUI();
                withinChest = false;
            }
            else
            {
                if (battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment.Contains(this))
                {
                    battleMaster.chestEquipmentManager.Unequip(this);  //Unequip
                    RemoveFromEquipment(equipSlot); //Remove from equipment
                }
                else
                {
                    battleMaster.chestEquipmentManager.Equip(this);  // Equip
                    RemoveFromInventory(this);  //Remove from inventory
                }
            }
        }
        else
        {
            if (battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment.Contains(this))
            {
                battleMaster.equipmentManager.Unequip(this);  //Unequip
                RemoveFromEquipment(equipSlot); //Remove from equipment
            }
            else
            {
                battleMaster.equipmentManager.Equip(this);  // Equip
                RemoveFromInventory(this);  //Remove from inventory
            }
        }
    }
}

public enum EquipmentSlot { Head, Chest, Arms, Legs, ArmSlot1, ArmSlot2, Ring1, Ring2, Neck }