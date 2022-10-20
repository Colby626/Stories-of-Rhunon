using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private Color startcolor;
    private SpriteRenderer characterSprite;
    private CharacterSheet character;
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
    public BattleMaster battleMaster;

    private bool isHighlighted = false;

    private void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        character = characterSprite.GetComponent<CharacterSheet>();
        startcolor = characterSprite.color;
        battleMaster = GameObject.FindGameObjectWithTag("BattleMaster").GetComponent<BattleMaster>();
    }

    //Highlights the character when the mouse is over them and displays their status menu
    void OnMouseOver()
    {
        if (!character.isDead && character.isPlayer)
        {
            ActivateStatus(character);
        }

        if (!character.isPlayer && !character.isDead)
        {
            character.transform.GetChild(0).gameObject.SetActive(true);
            overheadHealthBar.SetBarMax(character.MaxHealth);
            overheadHealthBar.SetBar(character.Health);
            overheadNameText.text = character.Name;
        }
        
        if (doMouseOver && !isHighlighted && !character.isDead)
        {
            isHighlighted = true;
        }

        if (character.isDead)
        {
            character.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (isHighlighted)
        {
            isHighlighted = false;
        }
    }

    //Remove status window and unhighlight
    private void OnMouseExit()
    {
        if (character.gameObject != battleMaster.currentCharacter)
        {
            //Display status of current character
            ActivateStatus(battleMaster.currentCharacter.GetComponent<CharacterSheet>());
        }
        character.transform.GetChild(0).gameObject.SetActive(false);
        isHighlighted = false;
    }

    public void ActivateStatus(CharacterSheet participant)
    {
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
