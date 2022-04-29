using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public StatBar healthBar;

    private CharacterSheet character;

    void Start()
    {
        character = GetComponent<CharacterSheet>();

        character.Health = character.MaxHealth;
        healthBar.SetMaxHealth(character.MaxHealth);
    }

    public void TakeDamage(int damage)
    {
        character.Health -= damage;
        if (character.Health <= 0)
        {
            //Die();
        }
    }
}
