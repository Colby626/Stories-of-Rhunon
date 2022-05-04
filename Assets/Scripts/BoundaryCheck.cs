using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryCheck : MonoBehaviour
{
    private GameObject blockerSprite;
    private SpriteRenderer sprite;
    [HideInInspector]
    public bool canClick;
    public bool entered = false;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        blockerSprite = transform.GetChild(0).gameObject;
        blockerSprite.SetActive(false);
        canClick = true;
    }

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (other.CompareTag("Boundary") || other.CompareTag("Participant"))
    //    {
    //        blockerSprite.SetActive(true);
    //        sprite.enabled = false;
    //        canClick = false;
    //    }
    //}

    private void Update()
    {

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Boundary") || collider.CompareTag("Participant"))
            {
                blockerSprite.SetActive(true);
                sprite.enabled = false;
                canClick = false;
            }
            else
            {
                blockerSprite.SetActive(false);
                sprite.enabled = true;
                canClick = true;
            }
        }
    }

    //private void OnTriggerStay2D(Collider2D other)
    //{
    //    entered = true;
    //}

    //private void OnTriggerExit2D(Collider2D other)
    //{
    //    entered = false;
    //    if (other.CompareTag("Boundary") || other.CompareTag("Participant"))
    //    {
    //        if (blockerSprite.activeSelf && !entered)
    //        {
    //            blockerSprite.SetActive(false);
    //            sprite.enabled = true;
    //            canClick = true;
    //        }
    //    }
    //}
}
