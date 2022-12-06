using System.Collections.Generic; //For lists
using UnityEngine;
using UnityEngine.UI;
using System.Linq; //For getting list counts
using TMPro; //For name text under turn order portraits
using System.Collections; //For IEnumerator like Timer

public class BattleMaster : MonoBehaviour
{
    #region Public Variables
    public int multiTurnThreshold = 50;
    public Text turn;
    public List<GameObject> turnOrder = new();
    public Texture2D cursorTexture;
    public Button attackButton;
    public Button nextTurnButton;
    public Button inventoryButton;
    [Tooltip("The time in seconds it waits before letting an AI run their turn")]
    public int delayBeforeEnemyTurns = 1;
    
    public List<Image> portraits;
    public List<TextMeshProUGUI> namesList;
    public List<StatBar> healthBars;
    public List<GameObject> characters;
    public List<GameObject> livingPlayers;
    public List<GameObject> livingEnemies;
    public CharacterSheet currentCharacter;
    public GameObject targetedPlayer;
    public GameObject targetedEnemy;
    public GameObject loseScreen;
    public GameObject winScreen;
    public GameObject battleHud;
    public GameObject status;
    [HideInInspector]
    public bool attackPressed = false;
    [HideInInspector]
    public bool attackDone = false;
    [HideInInspector]
    public bool turnOrderCalculated = false;
    public bool battleStarted = false;
    public int howFarInTheFutureYouCalculateTurnOrder = 50;
    public List<PathNode> moveableNodes;
    public GameMaster gameMaster;

    #endregion

    private List<GameObject> tempList = new();
    private GameObject[] characterArray;
    private int characterindex = 0;
    //public int turnCounter = 0; Edited for removing turnCounter
    public bool willWin = false;
    private CustomGrid grid;

    [Header("Inventory:")]
    public GameObject inventory;
    public InventoryUI inventoryUI;
    public Image equipmentPortrait;

    #region Leveling Variables
    [Header("Leveling:")]
    public GameObject levelUpButton;
    public GameObject levelUpPanel;
    public GameObject levelUpPortrait;
    public GameObject strengthText;
    public GameObject attunementText;
    public GameObject reflexesText;
    public GameObject speedText;
    public GameObject precisionText;
    public GameObject constitutionText;
    public GameObject enduranceText;
    #endregion

    private void Start()
    {
        if (battleStarted)
        {
            characterArray = GameObject.FindGameObjectsWithTag("Participant");
            characters = new List<GameObject>(characterArray);
            StartBattle(characters);
        }

        gameMaster = GameMaster.instance.GetComponent<GameMaster>();
        battleHud.SetActive(false);
        grid = gameMaster.grid;
    }

    void Update()
    {
        if (turnOrderCalculated && battleStarted)
        {
            if (currentCharacter.isDead)
            {
                NextTurn();
            }

            if (attackDone)
            {
                attackButton.interactable = false;
            }

            if (gameMaster.movedOnTurn)
            {
                ResetMovementLimit();
            }

            //checks if the turn counter is closer than the variable below from the furthest calculated the list has gone and if so calculates the list further
            //if (turnCounter >= (turnOrder.Count() - howFarInTheFutureYouCalculateTurnOrder)) //Edited for removing turnCounter
            //{
            //    CalculateTurnOrder();
            //}
            if (turnOrder.Count() < howFarInTheFutureYouCalculateTurnOrder) //Edited for removing turnCounter
            {
                CalculateTurnOrder();
            }

            //Displays the character to go's name on the screen
            turn.text = "It is " + currentCharacter.Name + "'s turn";


            if (!currentCharacter.isPlayer)
            {
                nextTurnButton.interactable = false;
                attackButton.interactable = false;
                inventoryButton.interactable = false;
            }
            else
            {
                nextTurnButton.interactable = true;
                attackButton.interactable = true;
                inventoryButton.interactable = true;
                currentCharacter.DisplayTurnMovement();
            }
        }
    }

    public void StartBattle(List<GameObject> participants)
    {
        characters = participants;
        battleStarted = true;
        battleHud.SetActive(true);
        StartingTurnOrder();

        foreach (GameObject character in characters)
        {
            if (character.GetComponent<CharacterSheet>().isPlayer)
            {
                livingPlayers.Add(character);
            }
        }

        //Gets a list of the enemies
        foreach (GameObject character in characters)
        {
            if (!character.GetComponent<CharacterSheet>().isPlayer)
            {
                livingEnemies.Add(character);
            }
        }

        //Displays the levelUpButton if the currentCharacter has enough XP to LevelUp
        if (currentCharacter.isPlayer && currentCharacter.characterStats.XP > currentCharacter.characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }

        //Display status of current character is they are a player
        if (currentCharacter.isPlayer)
        {
            currentCharacter.GetComponent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>());
        }

        //If the next person in line is not a player the AI will attack the nearest one
        if (!currentCharacter.isPlayer)
        {
            StartCoroutine(EnemyTurn());
        }

        LimitMovement();
    }

    private void LimitMovement()
    {
        int maxMoveDistance = currentCharacter.characterStats.Speed / 5;
        List<PathNode> tempPath = null;
        PathNode oNode = currentCharacter.GetComponentInParent<Movement>().occupyingNode;
        PathNode tempNode = null;
        for (int x = oNode.x - maxMoveDistance; x <= oNode.x + maxMoveDistance; x++)
        {
            for (int y = oNode.y - maxMoveDistance; y <= oNode.y + maxMoveDistance; y++)
            {
                tempNode = grid.GetGridObject(x, y);
                if (tempNode != null) //Position is walkable
                {
                    tempPath = gameMaster.GetComponent<Pathfinding>().FindPath(oNode, tempNode);
                    if (tempPath == null)
                    {
                        continue;
                    }
                    if (tempPath.Count <= maxMoveDistance) //It is within max move distance
                    {
                        moveableNodes.Add(tempNode);
                        tempNode.validMovePosition = true;
                        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Makes the color change only occur for players
                        {
                            tempNode.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            tempNode.transform.GetChild(2).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(2).GetComponent<SpriteRenderer>().color;
                        }
                    }
                }
            }
        }
    }

    private void ResetMovementLimit()
    {
        foreach (PathNode node in moveableNodes)
        {
            node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.whiteTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            node.transform.GetChild(2).GetComponent<SpriteRenderer>().color = grid.whiteTile.transform.GetChild(2).GetComponent<SpriteRenderer>().color;
            node.validMovePosition = false;
        }
        moveableNodes.Clear();
    }

    //Used for the button to go to the next turn
    public void NextTurn()
    {
        ResetMovementLimit();
        attackPressed = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        characterindex++;
        currentCharacter = turnOrder[1].GetComponent<CharacterSheet>(); //Edited for removing turnCounter
        attackDone = false;
        //turnCounter++; //Edited for removing turnCounter
        turnOrder.RemoveAt(0); //Edited for removing turnCounter
        attackButton.interactable = true;
        gameMaster.movedOnTurn = false;

        LimitMovement();
        LoadPortraits();

        //If the next person in line is not a player the AI will attack one of them at random
        if (!currentCharacter.isPlayer)
        {
            StartCoroutine(EnemyTurn()); //Delay for enemy turns
        }

        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.isPlayer && currentCharacter.characterStats.XP > currentCharacter.characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }

        if (currentCharacter.isPlayer)
        {
            currentCharacter.GetComponent<MouseOver>().ActivateStatus(currentCharacter); //Activates the status menu at the bottom to match the current character
        }
    }

    private void CalculateTurnOrder()
    {
        bool exit = false;

        //adjusts the priority in the turn order of all characters
        while (!exit)     //runs while nothing is in tempList
        {
            foreach (GameObject character in characters)
            {
                character.GetComponent<CharacterSheet>().turnOrderPriority += character.GetComponent<CharacterSheet>().characterStats.Speed;

                while (character.GetComponent<CharacterSheet>().turnOrderPriority >= multiTurnThreshold)
                {
                    character.GetComponent<CharacterSheet>().turnOrderPriority -= multiTurnThreshold;

                    turnOrder.Add(character);
                }
            }

            tempList = turnOrder.Intersect(characters).ToList();

            if (tempList.Count == characters.Count)
            {
                exit = true;
            }
        }
    } //do for instead of foreach

    private void StartingTurnOrder() //do for instead of foreach
    {
        bool exit = false;

        //adjusts the priority in the turn order of all characters
        while (!exit) //runs when nothing is in tempList
        {
            foreach (GameObject character in characters)
            {
                character.GetComponent<CharacterSheet>().turnOrderPriority += character.GetComponent<CharacterSheet>().characterStats.Reflexes;

                while (character.GetComponent<CharacterSheet>().turnOrderPriority >= multiTurnThreshold)
                {
                    character.GetComponent<CharacterSheet>().turnOrderPriority -= multiTurnThreshold;

                    turnOrder.Add(character);
                }
            }

            tempList = turnOrder.Intersect(characters).ToList();

            if (tempList.Count == characters.Count && turnOrder.Count >= portraits.Count) //Once each character is in the list once AND there are enough characters in the turn order to fill all the portraits 
            {
                exit = true;
            }
        }
        currentCharacter = turnOrder[0].GetComponent<CharacterSheet>();
        turnOrderCalculated = true;
        LoadPortraits();
    }

    public void Attack()
    {
        //Don't let the player attack more than once per turn 
        if (attackDone)
        {
            return;
        }

        //If the player is pressing the attack button after they already pressed it 
        if (attackPressed)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            attackPressed = false;
            return;
        }

        attackPressed = true;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OpenInventory()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        attackPressed = false;

        AudioManager.instance.Play("TurningPageInBookSound");
        if (currentCharacter.isPlayer)
        {
            battleHud.SetActive(false);
            inventory.SetActive(true);
            inventoryUI.GetComponent<InventoryUI>().UpdateUI();
            equipmentPortrait.sprite = currentCharacter.GetComponent<SpriteRenderer>().sprite;
            GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().UpdateEquipmentUI(); //Updates inventory to match the current character
        }
    }

    public void CloseInventory()
    {
        AudioManager.instance.Play("CloseBookSound");
        battleHud.SetActive(true);
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().ClearEquipmentUI();
        inventory.SetActive(false);
    }

    public void LevelUp()
    {
        AudioManager.instance.Play("TurningPageInBookSound");
        battleHud.SetActive(false);
        levelUpPanel.SetActive(true);
        levelUpPortrait.GetComponent<Image>().sprite = currentCharacter.GetComponent<SpriteRenderer>().sprite;
        strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + currentCharacter.characterStats.Strength.ToString();
        attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + currentCharacter.characterStats.Attunement.ToString();
        reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + currentCharacter.characterStats.Reflexes.ToString();
        speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + currentCharacter.characterStats.Speed.ToString();
        precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + currentCharacter.characterStats.Precision.ToString();
        constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + currentCharacter.characterStats.Constitution.ToString();
        enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + currentCharacter.characterStats.Endurance.ToString();
    }

    #region LevelUp Stat Functions
    public void LevelUpStrength()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Strength += 1;
        currentCharacter.characterStats.Damage += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + currentCharacter.characterStats.Strength.ToString();
    }

    public void LevelUpAttunement()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Attunement += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + currentCharacter.characterStats.Attunement.ToString();
    }

    public void LevelUpReflexes()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Reflexes += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + currentCharacter.characterStats.Reflexes.ToString();
    }

    public void LevelUpSpeed()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Speed += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + currentCharacter.characterStats.Speed.ToString();
    }

    public void LevelUpPrecision()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Precision += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + currentCharacter.characterStats.Precision.ToString();
    }

    public void LevelUpConstitution()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Constitution += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + currentCharacter.characterStats.Constitution.ToString();
    }

    public void LevelUpEndurance()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.characterStats.Endurance += 1;
        currentCharacter.characterStats.XP -= currentCharacter.characterStats.XPtoLevelUp;
        currentCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.characterStats.XP < currentCharacter.characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + currentCharacter.characterStats.Endurance.ToString();
    }
    #endregion

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(delayBeforeEnemyTurns);

        if (livingPlayers.Count > 0)
        {
            List<PathNode> closestPlayerPath = FindNearestPlayer(); //Also determines the targetedPlayer
            if (closestPlayerPath.Count == 1) //If the player is right next to the enemy (the 2 are the start node and end node)
            {
                //If the player is to the right of the enemy
                if (currentCharacter.transform.position.x - targetedPlayer.transform.position.x < 0)
                {
                    //If the enemy is facing left flip them
                    if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                //If the player is to the left or directly above/below the enemy
                if (currentCharacter.transform.position.x - targetedPlayer.transform.position.x >= 0)
                {
                    //If the enemy is facing right flip them
                    if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                AudioManager.instance.Play(currentCharacter.attackSound);
            }
            else
            {
                //Pathfind along that path until they are no longer validMovementNodes
                currentCharacter.GetComponentInParent<Movement>().MoveOnPath(closestPlayerPath);
            }
        }
    }

    private List<PathNode> FindNearestPlayer() //Searches for every player path, could be optimized by searching outward until hitting a player
    {
        List<PathNode> shortestPath = new List<PathNode> (new PathNode [500]); //Initalize the shortest path as 500 nodes
        List<PathNode> tempPath = new();
        foreach(GameObject player in livingPlayers)
        {
            tempPath = gameMaster.GetComponent<Pathfinding>().FindPath(currentCharacter.GetComponentInParent<Movement>().occupyingNode, player.GetComponentInParent<Movement>().occupyingNode);
            if (tempPath.Count < shortestPath.Count)
            {
                targetedPlayer = player;
                shortestPath = tempPath;
            }
        }
        PathNode lastNodeRemoved = null;
        foreach (PathNode node in shortestPath.ToList()) //I know shortestPath is a list, but it needs ToList() to not error
        {
            if (!node.validMovePosition) //Remove any node that is outside of the range of the enemy 
            {
                lastNodeRemoved = node;
                shortestPath.Remove(node);
            }
        }
        if (lastNodeRemoved != null)
        {
            if (lastNodeRemoved == gameMaster.partyNode)
            {
                currentCharacter.GetComponentInParent<Movement>().attackAtEnd = true;
            }
        }
        return shortestPath;
    }

    void LoadPortraits()
    {
        //Display portraits, names, and healths for the turn order
        for (int i = 0; i < portraits.Count(); i++)
        {
            portraits[i].sprite = turnOrder[i].GetComponent<CharacterSheet>().Portrait; //Edited for removing turnCounter (was turnCounter + i)
            portraits[i].preserveAspect = true;
            namesList[i].text = turnOrder[i].GetComponent<CharacterSheet>().Name;
            healthBars[i].SetBarMax(turnOrder[i].GetComponent<CharacterSheet>().MaxHealth);
            healthBars[i].SetBar(turnOrder[i].GetComponent<CharacterSheet>().Health); 
        }
    }

    public void Reset()
    {
        if (battleStarted)
        {
            battleStarted = false;
            turnOrderCalculated = false;
            //turnCounter = 0; Edited for removing turnCounter
            characterindex = 0;
            attackDone = false;
            attackPressed = false;
            gameMaster.GetComponent<GameMaster>().participants.Clear();
            turnOrder.Clear();
            characters.Clear();
            livingPlayers.Clear();
            livingEnemies.Clear();
            ResetMovementLimit();
        }
    }
}