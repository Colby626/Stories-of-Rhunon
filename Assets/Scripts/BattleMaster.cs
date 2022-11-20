using System.Collections.Generic; //For lists
using UnityEngine;
using UnityEngine.UI;
using System.Linq; //For getting list counts
using TMPro; //For name text under turn order portraits
using System.Collections; //For IEnumerator like Timer
using System.IO;

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
    public GameObject currentCharacter;
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
    public bool battleStarted;
    public int howFarInTheFutureYouCalculateTurnOrder = 50;
    public List<PathNode> moveableNodes;
    public GameMaster gameMaster;

    #endregion

    private List<GameObject> tempList = new();
    private GameObject[] characterArray;
    private int characterindex = 0;
    private int turnCounter = 0;
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

    public static BattleMaster instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        if (battleStarted)
        {
            characterArray = GameObject.FindGameObjectsWithTag("Participant");
            characters = new List<GameObject>(characterArray);
            StartBattle(characters);
        }

        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        grid = gameMaster.grid;
    }

    void Update()
    {
        if (turnOrderCalculated && battleStarted)
        {
            if (currentCharacter.GetComponent<CharacterSheet>().isDead)
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

            //checks if the turn counter is closer than 20 from the furthest calculated the list has gone and if so calculates the list further
            if (turnCounter >= (turnOrder.Count() - howFarInTheFutureYouCalculateTurnOrder))
            {
                CalculateTurnOrder();
            }

            //Displays the character to go's name on the screen
            turn.text = "It is " + currentCharacter.GetComponent<CharacterSheet>().Name + "'s turn";

            if (!currentCharacter.GetComponent<CharacterSheet>().isPlayer)
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
                currentCharacter.GetComponent<CharacterSheet>().DisplayTurnMovement();
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
        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer && currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }

        //Display status of current character is they are a player
        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            currentCharacter.GetComponentInParent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>());
        }

        //If the next person in line is not a player the AI will attack one of them at random
        if (!currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            StartCoroutine(EnemyTurn()); //Delay for enemy turns
        }

        LimitMovement();
    }

    private void LimitMovement()
    {
        int maxMoveDistance = currentCharacter.GetComponent<CharacterSheet>().characterStats.Speed / 5;
        for (int x = currentCharacter.GetComponentInParent<Movement>().occupyingNode.x - maxMoveDistance; x <= currentCharacter.GetComponentInParent<Movement>().occupyingNode.x + maxMoveDistance; x++)
        {
            for (int y = currentCharacter.GetComponentInParent<Movement>().occupyingNode.y - maxMoveDistance; y <= currentCharacter.GetComponentInParent<Movement>().occupyingNode.y + maxMoveDistance; y++)
            {
                if (grid.GetGridObject(x, y) != null) //Position is walkable
                {
                    if (gameMaster.GetComponent<Pathfinding>().FindPath(currentCharacter.GetComponentInParent<Movement>().occupyingNode, grid.GetGridObject(x, y)).Count <= maxMoveDistance) //It is within max move distance
                    {
                        moveableNodes.Add(grid.GetGridObject(x, y));
                        grid.GetGridObject(x, y).transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                        grid.GetGridObject(x, y).transform.GetChild(2).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(2).GetComponent<SpriteRenderer>().color;
                        grid.GetGridObject(x, y).validMovePosition = true;
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
        currentCharacter = turnOrder[characterindex];
        attackDone = false;
        turnCounter++;
        attackButton.interactable = true;
        gameMaster.movedOnTurn = false;

        LimitMovement();
        LoadPortraits();

        //If the next person in line is not a player the AI will attack one of them at random
        if (!currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            StartCoroutine(EnemyTurn()); //Delay for enemy turns
        }

        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer && currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }

        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            currentCharacter.GetComponentInParent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>()); //Activates the status menu at the bottom to match the current character
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
    }

    private void StartingTurnOrder()
    {
        bool exit = false;

        //adjusts the priority in the turn order of all characters
        while (!exit)     //runs when nothing is in tempList
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
        currentCharacter = turnOrder[0];
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
        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer)
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
        strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength.ToString();
        attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Attunement.ToString();
        reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Reflexes.ToString();
        speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Speed.ToString();
        precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision.ToString();
        constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Constitution.ToString();
        enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Endurance.ToString();
    }

    #region LevelUp Stat Functions
    public void LevelUpStrength()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Damage += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength.ToString();
    }

    public void LevelUpAttunement()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Attunement += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Attunement.ToString();
    }

    public void LevelUpReflexes()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Reflexes += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Reflexes.ToString();
    }

    public void LevelUpSpeed()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Speed += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Speed.ToString();
    }

    public void LevelUpPrecision()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision.ToString();
    }

    public void LevelUpConstitution()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Constitution += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Constitution.ToString();
    }

    public void LevelUpEndurance()
    {
        AudioManager.instance.Play("LevelupSound");
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Endurance += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

        //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP < currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            battleHud.SetActive(true);
            levelUpButton.SetActive(false);
            levelUpPanel.SetActive(false);
            AudioManager.instance.Play("CloseBookSound");
        }
        enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + currentCharacter.GetComponent<CharacterSheet>().characterStats.Endurance.ToString();
    }
    #endregion

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(delayBeforeEnemyTurns);

        if (livingPlayers.Count > 0)
        {
            List<PathNode> closestPlayerPath = FindNearestPlayer(); //Also determines the targetedPlayer
            if (closestPlayerPath.Count == 2) //If the player is right next to the enemy (the 2 are the start node and end node)
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
                AudioManager.instance.Play(currentCharacter.GetComponent<CharacterSheet>().attackSound);
            }
            //targetedPlayer = livingPlayers[Random.Range(0, livingPlayers.Count())];
            else
            {
                //Pathfind along that path until they are no longer validMovementNodes
                currentCharacter.GetComponentInParent<Movement>().MoveOnPath(closestPlayerPath);
            }
        }
    }

    private List<PathNode> FindNearestPlayer() //Searches for every player path, could be optimized by searching outward until hitting a player
    {
        List<PathNode> shortestPath = new List<PathNode> (new PathNode [500]); //Initalize the shorest path as 500 nodes
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

    private void Approach()
    {
        //Move towards the player's party
        //If player party within view range, wander = false;

        //Find the node closest to the player's party from the side the enemy is on and start moving towards it 
        //If player party leaves view range, wander = true;
    }

    void LoadPortraits()
    {
        //Display portraits, names, and healths for the turn order
        for (int i = 0; i < portraits.Count(); i++)
        {
            portraits[i].sprite = turnOrder[turnCounter + i].GetComponent<CharacterSheet>().Portrait;
            portraits[i].preserveAspect = true;
            namesList[i].text = turnOrder[turnCounter + i].GetComponent<CharacterSheet>().Name;
            healthBars[i].SetBarMax(turnOrder[turnCounter + i].GetComponent<CharacterSheet>().MaxHealth);
            healthBars[i].SetBar(turnOrder[turnCounter + i].GetComponent<CharacterSheet>().Health);
        }
    }

    public void Reset()
    {
        battleStarted = false;
        turnOrder.Clear();
        characters.Clear();
        livingEnemies.Clear();
        livingEnemies.Clear();
        turnOrderCalculated = false;
        turnCounter = 0;
        characterindex = 0;
    }
}