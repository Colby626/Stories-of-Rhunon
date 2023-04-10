using System; //for Enum.GetNames()
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public InventorySlot[] equipmentSlots;

    [HideInInspector]
    public BattleMaster battleMaster;

    int numSlots;

    void Start()
    {
        numSlots = Enum.GetNames(typeof(EquipmentSlot)).Length;
    }

    // Equip a new item
    public void Equip(Equipment newItem)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        Equipment oldItem;

        // Find out what slot the item fits in
        // and put it there.
        int slotIndex = (int)newItem.equipSlot;

        // If there was already an item in the slot
        // make sure to put it back in the inventory
        CharacterSheet character = (battleMaster.battleStarted) ? battleMaster.currentCharacter : battleMaster.defaultCharacter;
        if (character.characterEquipment[slotIndex] != null)
        {
            oldItem = character.characterEquipment[slotIndex];

            character.GetComponent<Inventory>().items.Add(oldItem);
        }

        character.characterEquipment[slotIndex] = newItem;

        if (battleMaster.chestMenu.activeSelf)
        {
            battleMaster.chestEquipmentManager.UpdateEquipmentUI(newItem.equipSlot, newItem);
            battleMaster.chestInventoryUI.UpdateUI();
        }
        else
        {
            battleMaster.equipmentManager.UpdateEquipmentUI(newItem.equipSlot, newItem);
            battleMaster.inventoryUI.UpdateUI();
        }

        AudioManager.instance.Play("EquipSound");

        //Increase damage or armor
        character.characterStats.Damage += newItem.damageIncrease;
        character.characterStats.Defense += newItem.damageNegation;
    }

    public void Unequip(Equipment oldItem)
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        EquipmentSlot slot = oldItem.equipSlot;
        switch (slot)
        {
            case EquipmentSlot.Head:
                {
                    equipmentSlots[0].item = null;
                    equipmentSlots[0].icon.enabled = false;
                    equipmentSlots[0].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Chest:
                {
                    equipmentSlots[1].item = null;
                    equipmentSlots[1].icon.enabled = false;
                    equipmentSlots[1].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Arms:
                {
                    equipmentSlots[2].item = null;
                    equipmentSlots[2].icon.enabled = false;
                    equipmentSlots[2].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Legs:
                {
                    equipmentSlots[3].item = null;
                    equipmentSlots[3].icon.enabled = false;
                    equipmentSlots[3].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.ArmSlot1:
                {
                    equipmentSlots[4].item = null;
                    equipmentSlots[4].icon.enabled = false;
                    equipmentSlots[4].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.ArmSlot2:
                {
                    equipmentSlots[5].item = null;
                    equipmentSlots[5].icon.enabled = false;
                    equipmentSlots[5].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Ring1:
                {
                    equipmentSlots[6].item = null;
                    equipmentSlots[6].icon.enabled = false;
                    equipmentSlots[6].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Ring2:
                {
                    equipmentSlots[7].item = null;
                    equipmentSlots[7].icon.enabled = false;
                    equipmentSlots[7].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
            case EquipmentSlot.Neck:
                {
                    equipmentSlots[8].item = null;
                    equipmentSlots[8].icon.enabled = false;
                    equipmentSlots[8].transform.GetChild(1).gameObject.SetActive(true);
                    break;
                }
        }
        CharacterSheet character = (battleMaster.battleStarted) ? battleMaster.currentCharacter : battleMaster.defaultCharacter;
        character.GetComponent<Inventory>().items.Add(oldItem);
        if (battleMaster.chestMenu.activeSelf)
        {
            battleMaster.chestInventoryUI.UpdateUI();
        }
        else
        {
            battleMaster.inventoryUI.UpdateUI();
        }
        AudioManager.instance.Play("EquipSound");

        character.characterEquipment[(int)slot] = null;
        UpdateEquipmentUI();

        //Reduce damage or armor
        character.characterStats.Damage -= oldItem.damageIncrease;
        character.characterStats.Defense -= oldItem.damageNegation;
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
                    equipmentSlots[0].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Chest:
                {
                    equipmentSlots[1].item = newItem;
                    equipmentSlots[1].icon.sprite = newItem.icon;
                    equipmentSlots[1].icon.enabled = true;
                    equipmentSlots[1].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Arms:
                {
                    equipmentSlots[2].item = newItem;
                    equipmentSlots[2].icon.sprite = newItem.icon;
                    equipmentSlots[2].icon.enabled = true;
                    equipmentSlots[2].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Legs:
                {
                    equipmentSlots[3].item = newItem;
                    equipmentSlots[3].icon.sprite = newItem.icon;
                    equipmentSlots[3].icon.enabled = true;
                    equipmentSlots[3].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.ArmSlot1:
                {
                    equipmentSlots[4].item = newItem;
                    equipmentSlots[4].icon.sprite = newItem.icon;
                    equipmentSlots[4].icon.enabled = true;
                    equipmentSlots[4].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.ArmSlot2:
                {
                    equipmentSlots[5].item = newItem;
                    equipmentSlots[5].icon.sprite = newItem.icon;
                    equipmentSlots[5].icon.enabled = true;
                    equipmentSlots[5].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Ring1:
                {
                    equipmentSlots[6].item = newItem;
                    equipmentSlots[6].icon.sprite = newItem.icon;
                    equipmentSlots[6].icon.enabled = true;
                    equipmentSlots[6].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Ring2:
                {
                    equipmentSlots[7].item = newItem;
                    equipmentSlots[7].icon.sprite = newItem.icon;
                    equipmentSlots[7].icon.enabled = true;
                    equipmentSlots[7].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
            case EquipmentSlot.Neck:
                {
                    equipmentSlots[8].item = newItem;
                    equipmentSlots[8].icon.sprite = newItem.icon;
                    equipmentSlots[8].icon.enabled = true;
                    equipmentSlots[8].transform.GetChild(1).gameObject.SetActive(false);
                    break;
                }
        }
    }

    public void UpdateEquipmentUI()
    {
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        for (int i = 0; i < numSlots; i++)
        {
            if (battleMaster.battleStarted)
            {
                Equipment currentCharacterEquipment = battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterEquipment[i];
                if (currentCharacterEquipment != null)
                {
                    equipmentSlots[i].item = currentCharacterEquipment;
                    equipmentSlots[i].icon.sprite = currentCharacterEquipment.icon;
                    equipmentSlots[i].icon.enabled = true;
                    equipmentSlots[i].transform.GetChild(1).gameObject.SetActive(false);
                }
            }
            else
            {
                Equipment defaultCharacterEquipment = battleMaster.defaultCharacter.GetComponent<CharacterSheet>().characterEquipment[i];
                if (defaultCharacterEquipment != null)
                {
                    equipmentSlots[i].item = defaultCharacterEquipment;
                    equipmentSlots[i].icon.sprite = defaultCharacterEquipment.icon;
                    equipmentSlots[i].icon.enabled = true;
                    equipmentSlots[i].transform.GetChild(1).gameObject.SetActive(false);
                }
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
            equipmentSlots[i].transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}