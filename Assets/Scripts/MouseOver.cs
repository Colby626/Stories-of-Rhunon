using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private CharacterSheet character;
    private Material characterMaterial;
    public bool doMouseOver = true;

    //public GameObject status;
    public Image portrait;
    public StatBar healthBar;
    public StatBar manaBar;
    public StatBar staminaBar;
    public StatBar overheadHealthBar;
    public TextMeshProUGUI overheadNameText;
    public Text nameText;
    public float animationTime = 1f;
    public Material highlightMaterial;
    public BattleMaster battleMaster;

    private bool isHighlighted = false;
    private PauseMenu pauseMenu;

    private void Start()
    {
        battleMaster = FindObjectOfType<BattleMaster>();
        if (GetComponent<CharacterSheet>()) //Is a player
        {
            character = GetComponent<CharacterSheet>();
            characterMaterial = GetComponent<SpriteRenderer>().material;
        }
        else //Is an enemy
        {
            character = transform.GetChild(0).GetComponent<CharacterSheet>();
            characterMaterial = transform.GetChild(0).GetComponent<SpriteRenderer>().material;
        }
        pauseMenu = FindObjectOfType<PauseMenu>();

        healthBar = battleMaster.status.transform.GetChild(1).GetComponent<StatBar>();
        manaBar = battleMaster.status.transform.GetChild(2).GetComponent<StatBar>();
        staminaBar = battleMaster.status.transform.GetChild(3).GetComponent<StatBar>();
        portrait = battleMaster.status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
        nameText = battleMaster.status.transform.GetChild(4).GetComponent<Text>();
        highlightMaterial = GameMaster.instance.highlightMaterial;

        if (character.isPlayer)
        {
            overheadHealthBar = transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
            overheadNameText = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        }
        else
        {
            overheadHealthBar = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<StatBar>();
            overheadNameText = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        }
    }

    //Highlights the character when the mouse is over them and displays their status menu
    void OnMouseOver()
    {
        if (battleMaster.battleStarted)
        {
            if (!character.isDead && character.isPlayer)
            {
                ActivateStatus(character);
            }

            if (!character.isPlayer && !character.isDead && !pauseMenu.gamePaused)
            {
                character.transform.GetChild(0).gameObject.SetActive(true);
                overheadHealthBar.SetBarMax(character.MaxHealth);
                overheadHealthBar.SetBar(character.Health);
                overheadNameText.text = character.Name;
            }
        
            if (!isHighlighted && !character.isDead && !pauseMenu.gamePaused)
            {
                character.GetComponent<SpriteRenderer>().material = highlightMaterial;
                isHighlighted = true;
            }

            if (character.isDead || pauseMenu.gamePaused)
            {
                character.transform.GetChild(0).gameObject.SetActive(false);
            }

            if (!character.isDead && !pauseMenu.gamePaused && !character.isPlayer)
            {
                character.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    private void OnMouseDown()
    {
        if (isHighlighted)
        {
            character.GetComponent<SpriteRenderer>().material = characterMaterial;
            isHighlighted = false;
        }

        character.OnMouseDown();
    }

    //Remove status window and unhighlight
    private void OnMouseExit()
    {
        if (battleMaster.battleStarted)
        {
            if (character.gameObject != battleMaster.currentCharacter && battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer)
            {
                //Display status of current character
                ActivateStatus(battleMaster.currentCharacter.GetComponent<CharacterSheet>());
            }
            character.transform.GetChild(0).gameObject.SetActive(false);
            character.GetComponent<SpriteRenderer>().material = characterMaterial;
            isHighlighted = false;
        }
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

        healthBar.SetBar(participant.Health);
        manaBar.SetBar(participant.Mana);
        staminaBar.SetBar(participant.Stamina);

        portrait.sprite = participant.Portrait;

        nameText.text = participant.Name;
    }
}
