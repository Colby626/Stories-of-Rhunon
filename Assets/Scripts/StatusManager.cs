using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public StatBar healthBar;
    public StatBar magicBar;
    public StatBar staminaBar;

    private CharacterSheet character;

    void Start()
    {
        character = GetComponent<CharacterSheet>();

        character.Health = character.MaxHealth;
        character.Mana = character.MaxMana;
        character.Stamina = character.MaxStamina;

        healthBar.SetBarMax(character.MaxHealth);
        magicBar.SetBarMax(character.MaxMana);
        staminaBar.SetBarMax(character.MaxStamina);
    }
}
