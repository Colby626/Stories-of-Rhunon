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
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().Equip(this);
    }
}

public enum EquipmentSlot { Head, Chest, Arms, Legs, ArmSlot1, ArmSlot2, Ring1, Ring2, Neck }