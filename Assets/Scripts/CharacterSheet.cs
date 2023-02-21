using System.Collections.Generic; //for Lists
using UnityEngine;
using System.Linq; //for livingEnemies.Count()
using TMPro;

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
    public int Strength; //Increases Damage
    [Range(0, 99)]
    public int Attunement; //Magical prowess
    [Range(0, 99)]
    public int Reflexes; //Determines how often it will be your turn
    [Range(0, 99)]
    public int Speed; //Determines how far you can travel in a turn
    [Range(0, 99)]
    public int Precision; //Critical chance
    [Range(0, 99)]
    public int Constitution; //Determines max health
    [Range(0, 99)]
    public int Endurance; //Determines max stamina
    public int Defense; //Decreases damage taken
    public int Damage; //Increases damage done
    public int XP;
    public int XPtoLevelUp;
}

/* For when there are more than one types of attacks
[System.Serializable]
public class CharacterAttacks
{
    [Header("Attacks:")]
    public List<string> attackNames;
}
*/

public class CharacterSheet : MonoBehaviour
{
    #region Variables
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
    //public CharacterAttacks characterAttacks; For when there are multiple types of attacks

    [Header("Programmer stuff:")]
    public bool isPlayer;
    [SerializeField]
    private bool killToWin = false;

    [HideInInspector]
    public int turnOrderPriority;
    [HideInInspector]
    public bool isDead = false;
    private BattleMaster battleMaster;
    private GameMaster gameMaster;

    #region Sound Variables
    private string noWhitespaceName;
    private string deathSound;
    [HideInInspector]
    public string attackSound;
    private string hitSound;
    #endregion
    #endregion

    private void Start()
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
        gameMaster = GameMaster.instance.GetComponent<GameMaster>();

        characterEquipment = new List<Equipment> { GetComponent<CharacterEquipment>().Head, GetComponent<CharacterEquipment>().Torso,
            GetComponent<CharacterEquipment>().Arms, GetComponent<CharacterEquipment>().Legs, GetComponent<CharacterEquipment>().ArmSlot1,
            GetComponent<CharacterEquipment>().ArmSlot2, GetComponent<CharacterEquipment>().Ring1, GetComponent<CharacterEquipment>().Ring2,
            GetComponent<CharacterEquipment>().Neck };
    }
    public void OnMouseDown()
    {
        //Checks if the player is clicking attack on a character
        if (!battleMaster.attackDone && !isPlayer)
        {
            if (GetComponentInParent<Movement>().occupyingNode.GetNeighborNodes().Contains(gameMaster.partyNode))
            {
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
                FindObjectOfType<AudioManager>().Play(battleMaster.currentCharacter.attackSound);
                battleMaster.attackDone = true;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                //Pathfind to the enemy the player clicked
                Debug.Log("clicking on an enemy when they are not in the tile next to the player");
                int maxMoveNodes = battleMaster.currentCharacter.characterStats.Speed / 5;
                gameMaster.party.GetComponent<Movement>().MoveOnPath(gameMaster.GetComponent<Pathfinding>().FindPath(GetComponentInParent<Movement>().occupyingNode, gameMaster.partyNode, maxMoveNodes));
            }
        }
    } //Must be public for MouseOver to access it

    private void FinishedAttack()
    {
        battleMaster.NextTurn();
    } //Called from animation event

    public void DealDamage() //Called from animation event
    {
        gameMaster.grid.gridClicked = false; //If not here, grid will register a click on the pathnodes below the enemy clicked
        if (!isPlayer)
        {
            battleMaster.targetedPlayer.GetComponent<CharacterSheet>().TakeDamage(characterStats.Damage + 1);
        }
        else
        {
            battleMaster.targetedEnemy.GetComponent<CharacterSheet>().TakeDamage(battleMaster.currentCharacter.characterStats.Damage + 1);
        }
    }

    public void TakeDamage(int damage)
    {
        bool critical = false;
        int rand = Random.Range(1, 100);
        if (rand > 100 - battleMaster.currentCharacter.characterStats.Precision && damage - characterStats.Defense > 0)
        {
            critical = true;
            damage = Mathf.RoundToInt(damage * 1.5f);
        }

        if (damage - characterStats.Defense > 0) //To make sure they don't heal what they negate
        {
            damage -= characterStats.Defense;
            Health -= damage;
        }
        else
        {
            damage = 0;
        }

        GetComponent<Animator>().SetTrigger("TakingHit");
        AudioManager.instance.Play(hitSound);
        PopUpDamage(damage, critical);

        if (Health <= 0)
        {
            Health = 0;
            GetComponent<Animator>().SetBool("Death", true);
        }
    }

    private void StartDie() //Called from animation event
    {
        battleMaster.livingPlayers.Remove(gameObject);
        battleMaster.livingEnemies.Remove(gameObject);
        battleMaster.characters.Remove(gameObject);
        while (battleMaster.turnOrder.Contains(gameObject))
        {
            battleMaster.turnOrder.Remove(gameObject);
        }
        isDead = true;
        AudioManager.instance.Play(deathSound); //Need to play this sound after the take hit animation and sound has finished
    }

    private void FinishDie()
    {
        if (battleMaster.currentCharacter.isPlayer && !isPlayer)
        {
            battleMaster.currentCharacter.characterStats.XP += characterStats.XP;
            if (!gameMaster.movedOnTurn)
            {
                GetComponentInParent<Movement>().occupyingNode.validMovePosition = true;
                battleMaster.moveableNodes.Add(GetComponentInParent<Movement>().occupyingNode);
                GetComponentInParent<Movement>().occupyingNode.transform.GetChild(1).gameObject.SetActive(true);
                GetComponentInParent<Movement>().occupyingNode.transform.GetChild(1).GetComponent<SpriteRenderer>().color = gameMaster.grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            }
        }

        GetComponentInParent<Movement>().occupyingNode.occupied = false;
        GetComponentInParent<Movement>().occupyingNode.occupyingAgent = null;

        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (battleMaster.currentCharacter.isPlayer && battleMaster.currentCharacter.characterStats.XP >= battleMaster.currentCharacter.characterStats.XPtoLevelUp)
        {
            battleMaster.levelUpButton.SetActive(true);
            battleMaster.levelUpCharacter = battleMaster.currentCharacter;
            AudioManager.instance.Play("SuccessSound");
        }
        else
        {
            battleMaster.levelUpButton.SetActive(false);
        }

        //If there are no more players alive, display the lose screen
        if (battleMaster.livingPlayers.Count() == 0)
        {
            gameMaster.EndBattle();
            battleMaster.battleStarted = false;
            gameObject.transform.parent.gameObject.SetActive(false);
            battleMaster.battleHud.SetActive(false);
            battleMaster.openInventoryButton.SetActive(false);
            battleMaster.levelUpButton.SetActive(false);
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
            gameMaster.EndBattle();
            battleMaster.battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", false);
            battleMaster.Reset();

            if (battleMaster.willWin) //Won the game
            {
                battleMaster.winScreen.SetActive(true);
                battleMaster.openInventoryButton.SetActive(false);
                battleMaster.levelUpButton.SetActive(false);
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
    } //Called from animation event

    private void PopUpDamage(int damage, bool critical)
    {
        GameObject damagePopUpInstance = Instantiate(battleMaster.damagePopUp, transform.position, Quaternion.identity);
        damagePopUpInstance.GetComponent<TextMeshPro>().text = damage.ToString();
        Vector2 direction = new (Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        damagePopUpInstance.GetComponent<Rigidbody2D>().AddForce(battleMaster.textSpeed * Time.deltaTime * direction, ForceMode2D.Impulse);
        if (isPlayer)
        {
            damagePopUpInstance.GetComponent<TextMeshPro>().color = (critical) ? Color.red : Color.yellow; 
        }
        else
        {
            damagePopUpInstance.GetComponent<TextMeshPro>().color = (critical) ? Color.cyan : Color.white;
        }
        Destroy(damagePopUpInstance, battleMaster.timeToDestroyFloatingDamageNumbers);
    }
}