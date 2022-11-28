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

        overheadHealthBar = transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
        healthBar = character.battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
        magicBar = character.battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
        staminaBar = character.battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();

        healthBar.SetBarMax(character.MaxHealth);
        magicBar.SetBarMax(character.MaxMana);
        staminaBar.SetBarMax(character.MaxStamina);
        overheadHealthBar.SetBarMax(character.MaxHealth);
    }
}
