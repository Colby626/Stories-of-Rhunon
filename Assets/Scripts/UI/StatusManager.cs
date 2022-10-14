using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public StatBar healthBar;
    public StatBar overheadHealthBar;
    public StatBar magicBar;
    public StatBar staminaBar;

    private CharacterSheet character;

    void Start()
    {
        character = GetComponent<CharacterSheet>();
        
        healthBar.SetBarMax(character.MaxHealth);
        magicBar.SetBarMax(character.MaxMana);
        staminaBar.SetBarMax(character.MaxStamina);
        overheadHealthBar.SetBarMax(character.MaxHealth);
    }
}
