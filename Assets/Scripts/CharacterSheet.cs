using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    public int Health;
    public int MaxHealth;
    public int Stamina;
    [Header("Proficiencies:")]
    public int FistProficiency;
    public int LightProficiency;
    public int One_handedProficiency;
    public int Two_handedProficiency;
    public int ReachProficiency;
    public int BowProficiency;
    public int CrossbowProficiency;
    public int ThrownProficiency;
    [Header("Stats:")]
    public int Strength;
    public int Attunement;
    public int Reflexes;
    public int Speed;
    public int Precision;
    public int Constitution;
    public int Grit;
    [Header("Equipment:")]
    public GameObject HandSlot1;
    public GameObject HandSlot2;
    public GameObject Head;
    public GameObject Torso;
    public GameObject Legs;
    public GameObject Feet;
    public GameObject Arms;
    public GameObject Ring1;
    public GameObject Ring2;
    public GameObject Neck;
    [Header("Programmer stuff:")]
    public float movementSpeed;

    private Color startcolor;

    //Bracelets and Rings go in the same slot

    //Highlights the character when the mouse is over them 
    void OnMouseEnter()
    {
        startcolor = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color * 1.5f;
    }

    //When the mouse leaves the character they will be unhighlighted
    void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = startcolor;
    }

    //Deals damage to this character
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if(Health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
}
    /*
Fist

Light: 
    daggers, clubs, sickle, whip(has reach)

One-handed:
    longswords, axes, maces, flails(ignore shields)

Two-handed:
    greatswords, greataxes, greatclubs, greathammers, quarterstaffs

Reach:
    spears, swordstaffs, poleaxes, scythes(ignore shields)

Bow:
    shortbows, longbows

Crossbow:
    light crossbows, crossbows, heavy crossbows

Thrown:
    javelins, throwing daggers, throwing axes, slings
     */
