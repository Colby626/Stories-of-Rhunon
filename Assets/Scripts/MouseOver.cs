using UnityEngine;
using UnityEngine.UI;

public class MouseOver : MonoBehaviour
{
    private Color startcolor;
    private SpriteRenderer characterSprite;
    private GameObject status;

    public Image portrait;
    public StatBar healthBar;

    private void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        startcolor = characterSprite.color;
        status = healthBar.transform.parent.gameObject;
    }

    //Highlights the character when the mouse is over them 
    void OnMouseEnter()
    {
        characterSprite.color *= 1.5f;

        SwapStatus();
    }

    //When the mouse leaves the character they will be unhighlighted
    void OnMouseExit()
    {
        characterSprite.color = startcolor;
        status.SetActive(false);
    }

    private void SwapStatus()
    {
        if (!status.activeSelf)
        {
            status.SetActive(true);
        }
        healthBar.SetHealth(characterSprite.GetComponent<CharacterSheet>().Health);
    }
}
