using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private Color startcolor;
    private SpriteRenderer characterSprite;
    private GameObject status;
    private CharacterSheet character;
    public bool doMouseOver = true;
    private Vector2 originalScale;

    public Image portrait;
    public StatBar healthBar;
    public StatBar manaBar;
    public StatBar staminaBar;
    public float animationTime = 1f;

    private void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        character = characterSprite.GetComponent<CharacterSheet>();
        startcolor = characterSprite.color;
        status = healthBar.transform.parent.gameObject;
    }

    //Highlights the character when the mouse is over them 
    void OnMouseEnter()
    {
        if (doMouseOver)
        {
            characterSprite.color *= 1.5f;

            SwapStatus();
        }
    }

    //When the mouse leaves the character they will be unhighlighted
    void OnMouseExit()
    {
        if (doMouseOver)
        {
            characterSprite.color = startcolor;
            status.SetActive(false);
        }
    }

    private void SwapStatus()
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
    }
}
