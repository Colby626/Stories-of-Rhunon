using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public StatBar healthBar;
    public StatBar overheadHealthBar;
    public StatBar magicBar;
    public StatBar staminaBar;
    public StatBar xpBar;

    private void Start()
    {
        CharacterSheet character = GetComponent<CharacterSheet>();
        BattleMaster battleMaster = FindObjectOfType<BattleMaster>();

        overheadHealthBar = transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
        healthBar = battleMaster.status.transform.GetChild(1).GetComponent<StatBar>();
        magicBar = battleMaster.status.transform.GetChild(2).GetComponent<StatBar>();
        staminaBar = battleMaster.status.transform.GetChild(3).GetComponent<StatBar>();
        xpBar = battleMaster.status.transform.GetChild(5).GetComponent<StatBar>();

        healthBar.SetBarMax(character.MaxHealth);
        magicBar.SetBarMax(character.MaxMana);
        staminaBar.SetBarMax(character.MaxStamina);
        overheadHealthBar.SetBarMax(character.MaxHealth);
    }
}
