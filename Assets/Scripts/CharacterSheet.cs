using System.Collections;
using System.Collections.Generic; //for Lists
using UnityEngine;
using System.Linq; //for livingEnemies.Count()
using UnityEngine.TextCore.Text;

[System.Serializable]
public class CharacterEquipment : Component
{
    [Header("Equipment:")]
    public Equipment Head;
    public Equipment Torso;
    public Equipment Arms;
    public Equipment Legs;
    public Equipment ArmSlot1;
    public Equipment ArmSlot2;
    public Equipment Ring1;
    public Equipment Ring2;
    public Equipment Neck;
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
    public int Strength; //Increases damage done
    public int Attunement; //Magical prowess
    public int Reflexes; //Determines how much you go at the start of combat
    public int Speed; //Determines how often you go
    public int Precision; //Critical chance
    public int Constitution; //Determines max health
    public int Endurance; //Determines max stamina
    [HideInInspector]
    public int Defense; //Decreases damage taken
    public int Damage;
    public int XP;
    public int XPtoLevelUp;
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
    public List<Equipment> characterEquipment;
    public CharacterAttacks characterAttacks;

    [Header("Programmer stuff:")]
    public float movementSpeed;
    public int turnOrderPriority;
    public BattleMaster battleMaster;
    public bool isPlayer;
    public bool isDead = false;

    private void Awake()
    {
        _startPos = transform.position; //For attack "animation"

        MaxHealth = characterStats.Constitution;
        MaxStamina = characterStats.Endurance;
        characterStats.Damage = characterStats.Strength;
        characterStats.XPtoLevelUp = 10; //This is the starting value for the amount of XP it takes to level up

        characterEquipment = new List<Equipment> { GetComponent<CharacterEquipment>().Head, GetComponent<CharacterEquipment>().Torso,
            GetComponent<CharacterEquipment>().Arms, GetComponent<CharacterEquipment>().Legs, GetComponent<CharacterEquipment>().ArmSlot1,
            GetComponent<CharacterEquipment>().ArmSlot2, GetComponent<CharacterEquipment>().Ring1, GetComponent<CharacterEquipment>().Ring2,
            GetComponent<CharacterEquipment>().Neck };
    }

    public void TakeDamage(int damage)
    {
        if (damage - characterStats.Defense > 0) //To make sure they don't heal what they negate
        {
            Health -= (damage - characterStats.Defense);
        }

        //Criticals deal 150% damage
        int rand = Random.Range(1, 100);
        if (rand > 100 - battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision)
        {
            Health -= (damage - characterStats.Defense) / 2;
        }

        GetComponent<Animator>().SetTrigger("TakingHit");

        if (Health <= 0)
        {
            StartDie();
        }
    }

    void StartDie()
    {
        battleMaster.livingPlayers.Remove(gameObject);
        battleMaster.livingEnemies.Remove(gameObject);
        battleMaster.characters.Remove(gameObject);
        while (battleMaster.turnOrder.Contains(gameObject))
        {
            battleMaster.turnOrder.Remove(gameObject);
        }
        isDead = true;

        GetComponent<Animator>().SetTrigger("Death"); 
    }

    public void FinishDie()
    {
        if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.XP += characterStats.XP;
        }

        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer && battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleMaster.levelUpButton.SetActive(true);
        }
        else
        {
            battleMaster.levelUpButton.SetActive(false);
        }

        Debug.Log("Finish Die");
        //If there are no more enemies, display the win screen
        if (battleMaster.livingEnemies.Count() == 0)
        {
            Debug.Log("Battle End");
            battleMaster.battleStarted = false;
            battleMaster.battleHud.SetActive(false);
            battleMaster.winScreen.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    public void OnMouseDown()
    {
        //Checks if the player is clicking attack on a character
        if (battleMaster.attackPressed && !isPlayer)
        {
            //battleMaster.currentCharacter.GetComponent<CharacterSheet>().Begin(); //Attack animation
            battleMaster.currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
            TakeDamage(battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Damage + 1);
            battleMaster.attackPressed = false;
            battleMaster.attackDone = true;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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