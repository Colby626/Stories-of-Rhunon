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
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        if (battleMaster.battleStarted)
        {
            if (battleMaster.currentCharacter.GetComponent<Inventory>().items.Contains(this))
            {
                GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().Equip(this);  // Equip
                RemoveFromInventory(this);  // Remove from inventory
            }

            else if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment.Contains(this))
            {
                GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().Unequip(this);  //Unequip
                RemoveFromEquipment(this.equipSlot); //Remove from equipment
            }
        }
        else
        {
            if (battleMaster.defaultCharacter.GetComponent<Inventory>().items.Contains(this))
            {
                GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().Equip(this);  // Equip
                RemoveFromInventory(this);  // Remove from inventory
            }

            else if (battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment.Contains(this))
            {
                GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().Unequip(this);  //Unequip
                RemoveFromEquipment(this.equipSlot); //Remove from equipment
            }
        }
    }
}

public enum EquipmentSlot { Head, Chest, Arms, Legs, ArmSlot1, ArmSlot2, Ring1, Ring2, Neck }