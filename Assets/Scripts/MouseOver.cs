using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private CharacterSheet character;
    private Image portrait;
    private StatBar xpBar;
    private StatBar healthBar;
    private StatBar manaBar;
    private StatBar staminaBar;
    private Text nameText;

    [HideInInspector]
    public Material characterMaterial;
    [HideInInspector]
    public StatBar overheadHealthBar;
    [HideInInspector]
    public TextMeshProUGUI overheadNameText;
    [HideInInspector]
    public Material highlightMaterial;
    [HideInInspector]
    public bool isHighlighted = false;

    private PauseMenu pauseMenu;
    private BattleMaster battleMaster;

    private void Start()
    {
        battleMaster = FindObjectOfType<BattleMaster>();
        character = GetComponent<CharacterSheet>();
        characterMaterial = GetComponent<SpriteRenderer>().material;
        pauseMenu = FindObjectOfType<PauseMenu>();

        portrait = battleMaster.status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
        healthBar = battleMaster.status.transform.GetChild(1).GetComponent<StatBar>();
        manaBar = battleMaster.status.transform.GetChild(2).GetComponent<StatBar>();
        staminaBar = battleMaster.status.transform.GetChild(3).GetComponent<StatBar>();
        nameText = battleMaster.status.transform.GetChild(4).GetComponent<Text>();
        xpBar = battleMaster.status.transform.GetChild(5).GetComponent<StatBar>();
        highlightMaterial = GameMaster.instance.highlightMaterial;

        overheadHealthBar = transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
        overheadNameText = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void ActivateStatus(CharacterSheet participant)
    {
        if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            battleMaster.status.SetActive(true);
        }

        healthBar.SetBarMax(participant.MaxHealth);
        manaBar.SetBarMax(participant.MaxMana);
        staminaBar.SetBarMax(participant.MaxStamina);
        xpBar.SetBarMax(participant.characterStats.XPtoLevelUp);

        healthBar.SetBar(participant.Health);
        manaBar.SetBar(participant.Mana);
        staminaBar.SetBar(participant.Stamina);
        xpBar.SetBar(participant.characterStats.XP);

        portrait.sprite = participant.Portrait;

        nameText.text = participant.Name;
    }
}
