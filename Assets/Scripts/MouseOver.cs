using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private Color startcolor;
    private SpriteRenderer characterSprite;
    private CharacterSheet character;
    public bool doMouseOver = true;

    public GameObject status;
    public Image portrait;
    public StatBar healthBar;
    public StatBar manaBar;
    public StatBar staminaBar;
    public Text nameText;
    public float animationTime = 1f;

    private bool isHighlighted = false;

    private void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        character = characterSprite.GetComponent<CharacterSheet>();
        startcolor = characterSprite.color;
        status = healthBar.transform.parent.gameObject;
    }

    //Highlights the character when the mouse is over them and displays their status menu
    void OnMouseOver()
    {
        if (!character.isDead)
        {
            ActivateStatus();
        }

        if (character.isDead)
        {
            status.SetActive(false);
        }
        
        if (doMouseOver && !isHighlighted)
        {
            characterSprite.color *= 1.5f;
            isHighlighted = true;
        }
    }

    //Remove status window and unhighlight
    private void OnMouseExit()
    {
        status.SetActive(false);
        characterSprite.color = startcolor;
        isHighlighted = false;
    }

    private void ActivateStatus()
    {
        if (!status.activeSelf)
        {
            status.SetActive(true);
        }

        healthBar.SetBarMax(character.MaxHealth);
        manaBar.SetBarMax(character.MaxMana);
        staminaBar.SetBarMax(character.MaxStamina);

        healthBar.SetBar(character.Health);
        manaBar.SetBar(character.Mana);
        staminaBar.SetBar(character.Stamina);

        portrait.sprite = character.Portrait;

        nameText.text = characterSprite.GetComponent<CharacterSheet>().Name;
    }
}
