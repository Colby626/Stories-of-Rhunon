using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class CharacterEquipment
{
    [Header("Equipment:")]
    public GameObject HandSlot1;
    public GameObject HandSlot2;
    public GameObject Head;
    public GameObject Torso;
    public GameObject Legs;
    public GameObject Feet;
    public GameObject Arms;
    public GameObject Ring1;
    public GameObject Ring2;
    public GameObject Neck;
}

[System.Serializable]
public class CharacterProficiencies
{
    [Header("Proficiencies:")]
    public int FistProficiency;
    public int LightProficiency;
    public int One_handedProficiency;
    public int Two_handedProficiency;
    public int ReachProficiency;
    public int BowProficiency;
    public int CrossbowProficiency;
    public int ThrownProficiency;
}

[System.Serializable]
public class CharacterStats
{
    [Header("Stats:")]
    public int Strength;
    public int Attunement;
    public int Reflexes;
    public int Speed;
    public int Precision;
    public int Constitution;
    public int Endurance;
}

[System.Serializable]
public class CharacterAttacks
{
    [Header("Attacks:")]
    public List<string> attackNames;
}

public class CharacterSheet : MonoBehaviour
{
    public string Name;
    public int Health;
    public int MaxHealth;
    public int Mana;
    public int MaxMana;
    public int Stamina;
    public int MaxStamina;
    public Sprite Portrait;
    public CharacterStats characterStats;
    public CharacterProficiencies characterProficiencies;
    public CharacterEquipment characterEquipment;
    public CharacterAttacks characterAttacks;

    [Header("Programmer stuff:")]
    public float movementSpeed;
    public int turnOrderPriority;

    //Bracelets and Rings go in the same slot

    void Die()
    {
        Destroy(this.gameObject);
    }
}
    /*
Fist

Light: 
    daggers, clubs, sickle, whip(has reach)

One-handed:
    longswords, axes, maces, flails(ignore shields)

Two-handed:
    greatswords, greataxes, greatclubs, greathammers, quarterstaffs

Reach:
    spears, swordstaffs, poleaxes, scythes(ignore shields)

Bow:
    shortbows, longbows

Crossbow:
    light crossbows, crossbows, heavy crossbows

Thrown:
    javelins, throwing daggers, throwing axes, slings
     */
