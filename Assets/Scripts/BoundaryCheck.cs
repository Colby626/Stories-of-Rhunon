using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryCheck : MonoBehaviour
{
    private GameObject blockerSprite;
    private SpriteRenderer sprite;
    [HideInInspector]
    public bool canClick;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        blockerSprite = transform.GetChild(0).gameObject;
        blockerSprite.SetActive(false);
        canClick = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boundary") || other.CompareTag("Participant"))
        {
            blockerSprite.SetActive(true);
            sprite.enabled = false;
            canClick = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boundary") || other.CompareTag("Participant"))
        {
            if (blockerSprite.activeSelf)
            {
                blockerSprite.SetActive(false);
                sprite.enabled = true;
                canClick = true;
            }
        }
    }
}
