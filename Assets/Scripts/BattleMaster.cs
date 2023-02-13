using System.Collections.Generic; //For lists
using UnityEngine;
using UnityEngine.UI;
using System.Linq; //For getting list counts
using TMPro; //For name text under turn order portraits
using System.Collections; //For IEnumerator like Timer

public class BattleMaster : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private int multiTurnThreshold = 50;
    [SerializeField]
    private int howFarInTheFutureYouCalculateTurnOrder = 50;
    [Tooltip("The time in seconds it waits before letting an AI run their turn")]
    [SerializeField]
    private int delayBeforeEnemyTurns = 1;
    public float timeToDestroyFloatingDamageNumbers = 1.5f;
    public float textSpeed = 150f;
    public GameObject damagePopUp;
    public bool battleStarted = false;

    public List<GameObject> turnOrder = new();
    public List<GameObject> characters;
    public List<GameObject> livingPlayers;
    public List<GameObject> livingEnemies;
    public CharacterSheet currentCharacter;
    public GameObject targetedPlayer;
    public GameObject targetedEnemy;

    [Header("GUI Elements")]
    [SerializeField]
    private Texture2D attackCursorTexture;
    [SerializeField]
    private Button attackButton;
    [SerializeField]
    private Button nextTurnButton;
    [SerializeField]
    private Button inventoryButton;
    [SerializeField]
    private Text turnText;
    [SerializeField]
    private List<Image> portraits;
    [SerializeField]
    private List<TextMeshProUGUI> namesList;
    [SerializeField]
    private List<StatBar> healthBars;
    [SerializeField]
    private List<CharacterSheet> characterList;
    private int characterListIndex = 0;
    private CharacterSheet levelUpCharacter;
    public CharacterSheet defaultCharacter;
    public GameObject loseScreen;
    public GameObject winScreen;
    public GameObject battleHud;
    public GameObject openInventoryButton;
    public GameObject nextCharacterButton;
    public GameObject previousCharacterButton;
    public GameObject status;
    
    private GameObject[] characterArray;
    private GameMaster gameMaster;
    private Pathfinding pathfinding;
    private CustomGrid grid;

    [HideInInspector]
    public List<PathNode> moveableNodes; //Must be public or becomes null
    [HideInInspector]
    public bool willWin = false;
    [HideInInspector]
    public bool attackPressed = false;
    [HideInInspector]
    public bool attackDone = false;
    [HideInInspector]
    public bool turnOrderCalculated = false;

    [Header("Inventory:")]
    public GameObject inventory;
    [SerializeField]
    private InventoryUI inventoryUI;
    [SerializeField]
    private Image equipmentPortrait;

    #region Leveling Variables
    [Header("Leveling:")]
    public GameObject levelUpButton;
    public GameObject levelUpPanel;
    [SerializeField]
    private GameObject levelUpPortrait;
    [SerializeField]
    private GameObject strengthText;
    [SerializeField]
    private GameObject attunementText;
    [SerializeField]
    private GameObject reflexesText;
    [SerializeField]
    private GameObject speedText;
    [SerializeField]
    private GameObject precisionText;
    [SerializeField]
    private GameObject constitutionText;
    [SerializeField]
    private GameObject enduranceText;
    #endregion
    #endregion

    private void Start()
    {
        if (battleStarted)
        {
            characterArray = GameObject.FindGameObjectsWithTag("Participant");
            characters = new List<GameObject>(characterArray);
            StartBattle(characters);
        }

        defaultCharacter = characterList[characterListIndex];
        gameMaster = GameMaster.instance.GetComponent<GameMaster>();
        pathfinding = FindObjectOfType<Pathfinding>().GetComponent<Pathfinding>(); 
        battleHud.SetActive(false);
        grid = FindObjectOfType<CustomGrid>().GetComponent<CustomGrid>();

        for (int i = 0; i < characterList.Count(); i++)
        {
            if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp)
            {
                levelUpButton.SetActive(true);
                levelUpCharacter = characterList[i];
            }
        }
    }

    private void Update()
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

            if (turnOrder.Count() < howFarInTheFutureYouCalculateTurnOrder)
            {
                CalculateTurnOrder();
            }

            //Displays the character to go's name on the screen
            turnText.text = "It is " + currentCharacter.Name + "'s turn";

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
            }
        }
    }

    #region Battle Functions
    public void StartBattle(List<GameObject> participants)
    {
        characters = participants;
        battleStarted = true;
        battleHud.SetActive(true);
        battleHud.GetComponentInChildren<Animator>().SetTrigger("BattleStart");
        battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
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
        if (currentCharacter.isPlayer && currentCharacter.characterStats.XP >= currentCharacter.characterStats.XPtoLevelUp)
        {
            levelUpCharacter = currentCharacter;
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

        //Add a delay so that participants can finish their move
        StartCoroutine(WaitHalfASecond());
    }

    IEnumerator WaitHalfASecond()
    {
        yield return new WaitForSeconds(0.5f);
        LimitMovement();
    }

    private void LimitMovement()
    {
        int maxMoveDistance = currentCharacter.characterStats.Speed / 5;
        List<PathNode> tempPath = null;
        PathNode oNode = currentCharacter.GetComponentInParent<Movement>().occupyingNode;
        PathNode tempNode = null;

        for (int x = oNode.x - maxMoveDistance; x <= oNode.x + maxMoveDistance; x++) //Check all the bottom nodes
        {
            tempNode = grid.GetGridObject(x, oNode.y - maxMoveDistance);
            if (tempNode != null) //Position is walkable
            {
                if (!tempNode.validMovePosition)
                {
                    tempPath = pathfinding.FindPath(tempNode, oNode, maxMoveDistance);
                    if (tempPath != null) //Necessary not to fail sometimes
                    {
                        foreach (PathNode node in tempPath)
                        {
                            moveableNodes.Add(node);
                            node.validMovePosition = true;
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Makes the color change only occur for players
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                        }
                    }
                }
            }

            tempNode = grid.GetGridObject(x, oNode.y + maxMoveDistance);
            if (tempNode != null) //Position is walkable
            {
                if (!tempNode.validMovePosition)
                {
                    tempPath = pathfinding.FindPath(tempNode, oNode, maxMoveDistance); 
                    if (tempPath != null) //Necessary not to fail sometimes
                    {
                        foreach (PathNode node in tempPath)
                        {
                            moveableNodes.Add(node);
                            node.validMovePosition = true;
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Makes the color change only occur for players
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                        }
                    }
                }
            }
        }

        for (int y = oNode.y - maxMoveDistance; y <= oNode.y + maxMoveDistance; y++) //Check all the left nodes
        {
            tempNode = grid.GetGridObject(oNode.x - maxMoveDistance, y);
            if (tempNode != null) //Position is walkable
            {
                if (!tempNode.validMovePosition)
                {
                    tempPath = pathfinding.FindPath(tempNode, oNode, maxMoveDistance); 
                    if (tempPath != null) //Necessary not to fail sometimes
                    {
                        foreach (PathNode node in tempPath)
                        {
                            moveableNodes.Add(node);
                            node.validMovePosition = true;
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Makes the color change only occur for players
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                        }
                    }
                }
            }
        }

        for (int x = oNode.x - maxMoveDistance; x <= oNode.x + maxMoveDistance; x++)
        {
            for (int y = oNode.y - maxMoveDistance; y <= oNode.y + maxMoveDistance; y++)
            {
                tempNode = grid.GetGridObject(x, y);
                if (tempNode != null) //Position is walkable
                {
                    if (!tempNode.validMovePosition)
                    {
                        tempPath = pathfinding.FindPath(tempNode, oNode, maxMoveDistance); //Here is where all the performance is eaten at battleStart
                        if (tempPath == null)
                        {
                            continue;
                        }
                        foreach (PathNode node in tempPath)
                        {
                            moveableNodes.Add(node);
                            node.validMovePosition = true;
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Makes the color change only occur for players
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
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
            node.validMovePosition = false;
        }
        moveableNodes.Clear();
    }

    public void NextTurn()
    {
        ResetMovementLimit();
        attackPressed = false;
        grid.gridClicked = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        currentCharacter = turnOrder[1].GetComponent<CharacterSheet>();
        attackDone = false;
        turnOrder.RemoveAt(0); 
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
        if (currentCharacter.isPlayer && currentCharacter.characterStats.XP >= currentCharacter.characterStats.XPtoLevelUp)
        {
            levelUpCharacter = currentCharacter;
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

    private void StartingTurnOrder()
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

            List<GameObject> tempList = turnOrder.Intersect(characters).ToList();

            if (tempList.Count == characters.Count && turnOrder.Count >= portraits.Count) //Once each character is in the list once AND there are enough characters in the turn order to fill all the portraits 
            {
                exit = true;
            }
        }
        currentCharacter = turnOrder[0].GetComponent<CharacterSheet>();
        turnOrderCalculated = true;
        LoadPortraits();
    }

    private void CalculateTurnOrder()
    {
        bool exit = false;

        //adjusts the priority in the turn order of all characters
        while (!exit)     //runs while nothing is in tempList
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

            List<GameObject> tempList = turnOrder.Intersect(characters).ToList();

            if (tempList.Count == characters.Count)
            {
                exit = true;
            }
        }
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(delayBeforeEnemyTurns);

        if (livingPlayers.Count > 0)
        {
            List<PathNode> closestPlayerPath = FindNearestPlayer(); //Also determines the targetedPlayer
            if (closestPlayerPath.Count == 1) //If the player is right next to the enemy
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

    private List<PathNode> FindNearestPlayer() //Searches for every player path, could be optimized by searching outward until hitting a player instead of comparing the distance of every path to each player
    {
        /* For when the party is split
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
        */
        int maxMoveDistance = currentCharacter.characterStats.Speed / 5;
        targetedPlayer = livingPlayers[Random.Range(0, livingPlayers.Count())]; //Randomly pick a player to attack
        List<PathNode> shortestPath = gameMaster.GetComponent<Pathfinding>().FindPath(gameMaster.partyNode, currentCharacter.GetComponentInParent<Movement>().occupyingNode, maxMoveDistance);
        if (pathfinding.lastNodeRemoved == null) //If the party is within the move range
        {
            currentCharacter.GetComponentInParent<Movement>().attackAtEnd = true;
        }
        /*
        else
        {
            Debug.Log("The party is outside of move range");
        }
        */

        return shortestPath;
    }

    private void LoadPortraits()
    {
        //Display portraits, names, and healths for the turn order
        for (int i = 0; i < portraits.Count(); i++)
        {
            portraits[i].sprite = turnOrder[i].GetComponent<CharacterSheet>().Portrait; 
            portraits[i].preserveAspect = true;
            namesList[i].text = turnOrder[i].GetComponent<CharacterSheet>().Name;
            healthBars[i].SetBarMax(turnOrder[i].GetComponent<CharacterSheet>().MaxHealth);
            healthBars[i].SetBar(turnOrder[i].GetComponent<CharacterSheet>().Health); 
        }
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
        Cursor.SetCursor(attackCursorTexture, Vector2.zero, CursorMode.Auto);
    } //Called from button

    #endregion

    #region Inventory Functions
    public void OpenInventory()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        attackPressed = false;

        AudioManager.instance.Play("TurningPageInBookSound");
        inventory.SetActive(true);
        openInventoryButton.SetActive(false);
        levelUpButton.SetActive(false);
        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().UpdateEquipmentUI(); //Updates inventory to match the right character

        if (!battleStarted)
        {
            nextCharacterButton.SetActive(true);
            previousCharacterButton.SetActive(true);
            equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        }
        else if (currentCharacter.isPlayer)
        {
            battleHud.SetActive(false);
            nextCharacterButton.SetActive(false);
            previousCharacterButton.SetActive(false);
            equipmentPortrait.sprite = currentCharacter.GetComponent<SpriteRenderer>().sprite;
        }
    } //Called from button

    public void CloseInventory()
    {
        AudioManager.instance.Play("CloseBookSound");
        if (battleStarted)
        {
            battleHud.SetActive(true);
            battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
            if (currentCharacter.characterStats.XP >= currentCharacter.characterStats.XPtoLevelUp)
            {
                levelUpButton.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < characterList.Count - 1; i++)
            {
                if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp)
                {
                    levelUpButton.SetActive(true);
                }
            }
        }
        openInventoryButton.SetActive(true);
        grid.gridClicked = false;
        gameMaster.hoveringOverButton = false;
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().ClearEquipmentUI();
        inventory.SetActive(false);
    } //Called from button

    public void NextCharacter() //remove buttons during battle
    {
        characterListIndex++;
        if (characterListIndex > characterList.Count - 1)
        {
            characterListIndex = 0;
        }
        defaultCharacter = characterList[characterListIndex];

        AudioManager.instance.Play("TurningPageInBookSound");
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().ClearEquipmentUI();

        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().UpdateEquipmentUI();
    } //Called from button
    public void PreviousCharacter() //remove buttons during battle
    {
        characterListIndex--;
        if (characterListIndex < 0)
        {
            characterListIndex = characterList.Count - 1;
        }
        defaultCharacter = characterList[characterListIndex];
        AudioManager.instance.Play("TurningPageInBookSound");
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().ClearEquipmentUI();

        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().UpdateEquipmentUI();
    } //Called from button

    #endregion

    #region LevelUp Functions
    public void LevelUp()
    {
        AudioManager.instance.Play("TurningPageInBookSound");
        battleHud.SetActive(false);
        openInventoryButton.SetActive(false);
        levelUpButton.SetActive(false);
        levelUpPanel.SetActive(true);
        levelUpPortrait.GetComponent<Image>().sprite = levelUpCharacter.GetComponent<SpriteRenderer>().sprite;
        strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + levelUpCharacter.characterStats.Strength.ToString();
        attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + levelUpCharacter.characterStats.Attunement.ToString();
        reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + levelUpCharacter.characterStats.Reflexes.ToString();
        speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + levelUpCharacter.characterStats.Speed.ToString();
        precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + levelUpCharacter.characterStats.Precision.ToString();
        constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + levelUpCharacter.characterStats.Constitution.ToString();
        enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + levelUpCharacter.characterStats.Endurance.ToString();
    } //Called from button

    public void LevelUpStrength()
    {
        if (levelUpCharacter.characterStats.Strength < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Strength += 1;
            levelUpCharacter.characterStats.Damage += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            strengthText.GetComponent<TextMeshProUGUI>().text = "Strength: " + levelUpCharacter.characterStats.Strength.ToString();
        }
    } //Called from button

    public void LevelUpAttunement()
    {
        if (levelUpCharacter.characterStats.Attunement < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Attunement += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            attunementText.GetComponent<TextMeshProUGUI>().text = "Attunement: " + levelUpCharacter.characterStats.Attunement.ToString();
        }
    } //Called from button

    public void LevelUpReflexes()
    {
        if (levelUpCharacter.characterStats.Reflexes < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Reflexes += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            reflexesText.GetComponent<TextMeshProUGUI>().text = "Reflexes: " + levelUpCharacter.characterStats.Reflexes.ToString();
        }
    } //Called from button

    public void LevelUpSpeed()
    {
        if (levelUpCharacter.characterStats.Speed < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Speed += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            speedText.GetComponent<TextMeshProUGUI>().text = "Speed: " + levelUpCharacter.characterStats.Speed.ToString();
        }
    } //Called from button

    public void LevelUpPrecision()
    {
        if (levelUpCharacter.characterStats.Precision < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Precision += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            precisionText.GetComponent<TextMeshProUGUI>().text = "Precision: " + levelUpCharacter.characterStats.Precision.ToString();
        }
    } //Called from button

    public void LevelUpConstitution()
    {
        if (levelUpCharacter.characterStats.Constitution < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Constitution += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            constitutionText.GetComponent<TextMeshProUGUI>().text = "Constitution: " + levelUpCharacter.characterStats.Constitution.ToString();
        }
    } //Called from button

    public void LevelUpEndurance()
    {
        if (levelUpCharacter.characterStats.Endurance < 99)
        {
            AudioManager.instance.Play("LevelupSound");
            levelUpCharacter.characterStats.Endurance += 1;
            levelUpCharacter.characterStats.XP -= levelUpCharacter.characterStats.XPtoLevelUp;
            levelUpCharacter.characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again

            //Keep the levelup screen up if the currentCharacter has more XP than they need to level up
            if (levelUpCharacter.characterStats.XP < levelUpCharacter.characterStats.XPtoLevelUp)
            {
                CloseLevelUpPanel();
            }
            enduranceText.GetComponent<TextMeshProUGUI>().text = "Endurance: " + levelUpCharacter.characterStats.Endurance.ToString();
        }
    } //Called from button

    public void CloseLevelUpPanel()
    {
        if (battleStarted)
        {
            battleHud.SetActive(true);
            battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
        }
        else
        {
            for (int i = 0; i < characterList.Count(); i++)
            {
                if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp)
                {
                    levelUpButton.SetActive(true);
                    levelUpCharacter = characterList[i];
                }
            }
        }
        grid.gridClicked = false;
        gameMaster.hoveringOverButton = false;
        openInventoryButton.SetActive(true);
        levelUpPanel.SetActive(false);
        AudioManager.instance.Play("CloseBookSound");
    }
    #endregion

    public void Reset()
    {
        if (battleStarted)
        {
            battleStarted = false;
            turnOrderCalculated = false;
            attackDone = false;
            attackPressed = false;
            turnOrder.Clear();
            characters.Clear();
            livingPlayers.Clear();
            livingEnemies.Clear();
            ResetMovementLimit();
        }
    }
}