using System; //for Enum.GetNames()
using System.Collections.Generic; //for Lists
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public InventorySlot[] equipmentSlots;

    Inventory inventory;
    InventoryUI inventoryUI;

    BattleMaster battleMaster;
    int numSlots;

    void Awake()
    {
        inventoryUI = GameObject.FindGameObjectWithTag("InventoryUI").GetComponent<InventoryUI>();
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        inventory = battleMaster.currentCharacter.GetComponent<Inventory>();
        numSlots = Enum.GetNames(typeof(EquipmentSlot)).Length;
    }

    // Equip a new item
    public void Equip(Equipment newItem)
    {
        Equipment oldItem;

        // Find out what slot the item fits in
        // and put it there.
        int slotIndex = (int)newItem.equipSlot;

        // If there was already an item in the slot
        // make sure to put it back in the inventory
        if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[slotIndex] != null)
        {
            oldItem = battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[slotIndex];

            inventory.Add(oldItem);
        }

        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[slotIndex] = newItem;

        EquipmentSlot slot = newItem.equipSlot;
        UpdateEquipmentUI(slot, newItem);

        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
        battleMaster.currentCharacter.GetComponent<Inventory>().items.Remove(newItem);
        inventoryUI = GameObject.FindGameObjectWithTag("InventoryUI").GetComponent<InventoryUI>();
        inventoryUI.UpdateUI();

        //Increase damage or armor
        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength += newItem.damageIncrease;
        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Defense += newItem.damageNegation;
    }

    public void Unequip(Equipment oldItem)
    {
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();

        EquipmentSlot slot = oldItem.equipSlot;
        switch (slot)
        {
            case EquipmentSlot.Head:
                {
                    equipmentSlots[0].item = null;
                    equipmentSlots[0].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Chest:
                {
                    equipmentSlots[1].item = null;
                    equipmentSlots[1].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Arms:
                {
                    equipmentSlots[2].item = null;
                    equipmentSlots[2].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Legs:
                {
                    equipmentSlots[3].item = null;
                    equipmentSlots[3].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.ArmSlot1:
                {
                    equipmentSlots[4].item = null;
                    equipmentSlots[4].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.ArmSlot2:
                {
                    equipmentSlots[5].item = null;
                    equipmentSlots[5].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Ring1:
                {
                    equipmentSlots[6].item = null;
                    equipmentSlots[6].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Ring2:
                {
                    equipmentSlots[7].item = null;
                    equipmentSlots[7].icon.enabled = false;
                    break;
                }
            case EquipmentSlot.Neck:
                {
                    equipmentSlots[8].item = null;
                    equipmentSlots[8].icon.enabled = false;
                    break;
                }
        }
        battleMaster.currentCharacter.GetComponent<Inventory>().items.Add(oldItem);
        inventoryUI.UpdateUI();

        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[(int)slot] = null;
        UpdateEquipmentUI();

        //Reduce damage or armor
        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength -= oldItem.damageIncrease;
        battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Defense -= oldItem.damageNegation;
    }

    private void UpdateEquipmentUI(EquipmentSlot slot, Equipment newItem)
    {
        switch (slot)
        {
            case EquipmentSlot.Head:
                {
                    equipmentSlots[0].item = newItem;
                    equipmentSlots[0].icon.sprite = newItem.icon;
                    equipmentSlots[0].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Chest:
                {
                    equipmentSlots[1].item = newItem;
                    equipmentSlots[1].icon.sprite = newItem.icon;
                    equipmentSlots[1].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Arms:
                {
                    equipmentSlots[2].item = newItem;
                    equipmentSlots[2].icon.sprite = newItem.icon;
                    equipmentSlots[2].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Legs:
                {
                    equipmentSlots[3].item = newItem;
                    equipmentSlots[3].icon.sprite = newItem.icon;
                    equipmentSlots[3].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.ArmSlot1:
                {
                    equipmentSlots[4].item = newItem;
                    equipmentSlots[4].icon.sprite = newItem.icon;
                    equipmentSlots[4].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.ArmSlot2:
                {
                    equipmentSlots[5].item = newItem;
                    equipmentSlots[5].icon.sprite = newItem.icon;
                    equipmentSlots[5].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Ring1:
                {
                    equipmentSlots[6].item = newItem;
                    equipmentSlots[6].icon.sprite = newItem.icon;
                    equipmentSlots[6].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Ring2:
                {
                    equipmentSlots[7].item = newItem;
                    equipmentSlots[7].icon.sprite = newItem.icon;
                    equipmentSlots[7].icon.enabled = true;
                    break;
                }
            case EquipmentSlot.Neck:
                {
                    equipmentSlots[8].item = newItem;
                    equipmentSlots[8].icon.sprite = newItem.icon;
                    equipmentSlots[8].icon.enabled = true;
                    break;
                }
        }
    }

    public void UpdateEquipmentUI()
    {
        for (int i = 0; i < numSlots; i++)
        {
            if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[i] != null)
            {
                equipmentSlots[i].item = battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[i];
                equipmentSlots[i].icon.sprite = battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[i].icon;
                equipmentSlots[i].icon.enabled = true;
            }
        }
    }

    public void ClearEquipmentUI()
    {
        for (int i = 0; i < numSlots; i++)
        {
            equipmentSlots[i].item = null;
            equipmentSlots[i].icon.sprite = null;
            equipmentSlots[i].icon.enabled = false;
        }
    }
}