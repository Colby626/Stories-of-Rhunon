using System.Collections;
using System.Collections.Generic; //for Lists
using UnityEngine;
using System.Linq; //for livingEnemies.Count()

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
    [Range(0, 99)]
    public int Strength; //Increases damage done
    [Range(0, 99)]
    public int Attunement; //Magical prowess
    [Range(0, 99)]
    public int Reflexes; //Determines how much you go at the start of combat
    [Range(0, 99)]
    public int Speed; //Determines how often you go
    [Range(0, 99)]
    public int Precision; //Critical chance
    [Range(0, 99)]
    public int Constitution; //Determines max health
    [Range(0, 99)]
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
    #region Public Variables
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
    public float moveDistance;
    public int turnOrderPriority;
    public bool isPlayer;
    public bool killToWin = false;
    public bool isDead = false;
    #endregion

    public BattleMaster battleMaster;

    #region Sound Variables
    private string noWhitespaceName;
    private string deathSound;
    [HideInInspector]
    public string attackSound;
    private string hitSound;
    #endregion

    private void Awake()
    {
        noWhitespaceName = Name;
        while (noWhitespaceName.Contains(" "))
        {
            noWhitespaceName = noWhitespaceName.Replace(" ", string.Empty);
        }
        deathSound = noWhitespaceName + "DeathSound";
        attackSound = noWhitespaceName + "AttackSound";
        hitSound = noWhitespaceName + "HitSound";

        MaxHealth = characterStats.Constitution;
        MaxStamina = characterStats.Endurance;
        characterStats.Damage = characterStats.Strength;
        characterStats.XPtoLevelUp = 10; //This is the starting value for the amount of XP it takes to level up
        battleMaster = FindObjectOfType<BattleMaster>();

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
        if (rand > 100 - battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision && damage - characterStats.Defense > 0)
        {
            Debug.Log("Critical Hit!");
            Health -= (damage - characterStats.Defense) / 2;
        }

        GetComponent<Animator>().SetTrigger("TakingHit");
        AudioManager.instance.Play(hitSound);

        if (Health <= 0)
        {
            GetComponent<Animator>().SetBool("Death", true);
        }
    }

    void StartDie() //Called from animation event
    {
        battleMaster.livingPlayers.Remove(gameObject);
        battleMaster.livingEnemies.Remove(gameObject);
        battleMaster.characters.Remove(gameObject);
        while (battleMaster.turnOrder.Contains(gameObject))
        {
            if (battleMaster.turnOrder.IndexOf(gameObject) < battleMaster.turnCounter) //If removing an enemy from the list is to the left of the current turn counter, also move the turn counter back one
            {
                battleMaster.turnCounter -= 1;
            }
            battleMaster.turnOrder.Remove(gameObject);
        }
        isDead = true;
        AudioManager.instance.Play(deathSound); //Need to play this sound after the take hit animation and sound has finished
    }

    public void FinishDie()
    {
        GetComponentInParent<Movement>().occupyingNode.occupied = false;
        GetComponentInParent<Movement>().occupyingNode.occupyingAgent = null;
        GetComponentInParent<Movement>().occupyingNode.transform.GetChild(1).gameObject.SetActive(true);
        GetComponentInParent<Movement>().occupyingNode.transform.GetChild(2).gameObject.SetActive(true);

        if (battleMaster.currentCharacter.GetComponent<CharacterSheet>().isPlayer && !isPlayer)
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

        //If there are no more players alive, display the lose screen
        if (battleMaster.livingPlayers.Count() == 0)
        {
            battleMaster.gameMaster.GetComponent<GameMaster>().EndBattle();
            battleMaster.battleStarted = false;
            gameObject.transform.parent.gameObject.SetActive(false);
            battleMaster.battleHud.SetActive(false);
            battleMaster.loseScreen.SetActive(true);
            AudioManager.instance.Stop("BattleMusic");
            AudioManager.instance.Play("LoseSound");
        }

        if (killToWin) //An enemy marked with killToWin was killed
        {
            battleMaster.willWin = true;
        }

        //If there are no more enemies, display the win screen
        if (battleMaster.livingEnemies.Count() == 0)
        {
            battleMaster.gameMaster.GetComponent<GameMaster>().EndBattle();
            battleMaster.battleHud.SetActive(false);
            battleMaster.Reset();

            if (battleMaster.willWin) //Won the game
            {
                battleMaster.winScreen.SetActive(true);
                AudioManager.instance.Stop("BattleMusic");
                AudioManager.instance.Play("WinSound");
            }
            else //Won the battle
            {
                AudioManager.instance.Stop("BattleMusic");
                AudioManager.instance.Play("ExploringMusic");
            }
        }        

        if (!isPlayer) 
        { 
            gameObject.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        if (GetComponentInParent<MouseOver>()) //MouseOver should only be on the character after a battle has started, this prevents an error if you click on them outside of battle
        {
            //Checks if the player is clicking attack on a character
            if (battleMaster.attackPressed && !isPlayer && battleMaster.battleStarted && GetComponentInParent<Movement>().occupyingNode.GetNeighborNodes().Contains(battleMaster.gameMaster.partyNode))
            {
                battleMaster.attackPressed = false;
                battleMaster.targetedEnemy = gameObject;
                //If the enemy is to the right of the player
                if (battleMaster.currentCharacter.transform.position.x - transform.position.x < 0)
                {
                    //If the player is facing left flip them
                    if (battleMaster.currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if (battleMaster.currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                //If the enemy is to the left or directly above/below the player
                if (battleMaster.currentCharacter.transform.position.x - transform.position.x >= 0)
                {
                    //If the player is facing right flip them
                    if (battleMaster.currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if(battleMaster.currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        battleMaster.currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                battleMaster.currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                FindObjectOfType<AudioManager>().Play(battleMaster.currentCharacter.GetComponent<CharacterSheet>().attackSound);
                battleMaster.attackDone = true;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    public void DealDamage()
    {
        battleMaster.gameMaster.grid.gridClicked = false; //If not here, grid will register a click on the pathnodes below the enemy clicked
        if (!isPlayer)
        {
            battleMaster.targetedPlayer.GetComponent<CharacterSheet>().TakeDamage(characterStats.Damage + 1);
        }
        else
        {
            battleMaster.targetedEnemy.GetComponent<CharacterSheet>().TakeDamage(battleMaster.currentCharacter.GetComponent<CharacterSheet>().characterStats.Damage + 1);
        }
    }

    public void FinishedAttack()
    {
        battleMaster.NextTurn();
    }

    public void DisplayTurnMovement()
    {
        moveDistance = characterStats.Speed / 5; //Can move speed / 5 number of spaces when it is their turn
    }
}