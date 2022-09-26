using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; //for livingEnemies.Count()
using UnityEngine.InputSystem.Processors;

[System.Serializable]
public class CharacterEquipment
{
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
}

[System.Serializable]
public class CharacterProficiencies
{
    [Header("Proficiencies:")]
    public int FistProficiency;
    public int LightProficiency;
    public int One_handedProficiency;
    public int Two_handedProficiency;
    public int ReachProficiency;
    public int BowProficiency;
    public int CrossbowProficiency;
    public int ThrownProficiency;
}

[System.Serializable]
public class CharacterStats
{
    [Header("Stats:")]
    public int Strength;
    public int Attunement;
    public int Reflexes;
    public int Speed;
    public int Precision;
    public int Constitution;
    public int Endurance;
}

[System.Serializable]
public class CharacterAttacks
{
    [Header("Attacks:")]
    public List<string> attackNames;
}

public class CharacterSheet : MonoBehaviour
{
    public string Name;
    public int Health;
    public int MaxHealth;
    public int Mana;
    public int MaxMana;
    public int Stamina;
    public int MaxStamina;
    public Sprite Portrait;
    public CharacterStats characterStats;
    public CharacterProficiencies characterProficiencies;
    public CharacterEquipment characterEquipment;
    public CharacterAttacks characterAttacks;

    [Header("Programmer stuff:")]
    public float movementSpeed;
    public int turnOrderPriority;
    public BattleMaster battleMaster;
    public bool isPlayer;
    public bool isDead = false;

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        battleMaster.livingPlayers.Remove(gameObject);
        battleMaster.livingEnemies.Remove(gameObject);
        battleMaster.characters.Remove(gameObject);
        while (battleMaster.turnOrder.Contains(gameObject))
        {
            battleMaster.turnOrder.Remove(gameObject);
        }
        isDead = true;
        if (GetComponent<MouseOver>().status.activeSelf)
        {
            GetComponent<MouseOver>().status.SetActive(false);
        }

        //If there are no more enemies, display the win screen
        if (battleMaster.livingEnemies.Count() == 0)
        {
            battleMaster.battleStarted = false;
            battleMaster.winScreen.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    public void OnMouseDown()
    {
        //Checks if the player is clicking attack on a character
        if (battleMaster.attackPressed && !isPlayer)
        {
            battleMaster.currentCharacter.GetComponent<CharacterSheet>().Begin(); //Attack animation
            TakeDamage(battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength + 1);
            battleMaster.attackPressed = false;
            battleMaster.attackDone = true;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            if (battleMaster.livingEnemies.Count() == 0)
            {
                //Display win screen
            }
        }
    }

/////////////////////// Everything below here is my "attack animation" of shaking
 
    [Header("Info")]
    private Vector3 _startPos;
    private float _timer;
    private Vector3 _randomPos;

    [Header("Shake Settings")]
    [Range(0f, 2f)]
    public float _time = 0.2f;
    [Range(0f, 2f)]
    public float _distance = 0.1f;


    private void Awake()
    {
        _startPos = transform.position;
    }

    public void Begin()
    {
        StopAllCoroutines();
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        _timer = 0f;

        while (_timer < _time)
        {
            _timer += Time.deltaTime;

            _randomPos = _startPos + (Random.insideUnitSphere * _distance);

            transform.position = _randomPos;

            yield return null;
        }

        transform.position = _startPos;
    }

}