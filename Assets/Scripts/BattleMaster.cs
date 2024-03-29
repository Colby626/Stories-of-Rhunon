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
    [Tooltip("The amount of turnOrderPriority that gets reduced when a new battler joins. 0 makes it to where people currently in battle have turn order priority advantage, higher numbers to a max of the multiTurnThreshold makes this advantage go away.")]
    [SerializeField]
    private int turnOrderPriorityReduction;
    [Tooltip("The amount of turnOrderPriority that is negated from 0 on a battler joining an in progress battle")]
    [SerializeField]
    private int turnOrderPriorityDisadvantage;
    [SerializeField]
    private int howFarInTheFutureYouCalculateTurnOrder = 50;
    [Tooltip("The time in seconds it waits before letting an AI run their turn")]
    [SerializeField]
    private int delayBeforeEnemyTurns = 1;
    public float timeToDestroyFloatingDamageNumbers = 1.5f;
    public float textSpeed = 150f;
    public GameObject damagePopUp;
    public bool battleStarted = false;
    [Tooltip("This is multiplied by furthestAnyoneCanMove and if someone is further away than that then they leave battle")]
    public float giveUpDistance = 1.5f;
    [HideInInspector]
    public bool inventoryOpen = false;
    [HideInInspector]
    public bool levelupScreenOpen = false;

    public List<GameObject> turnOrder = new();
    public List<GameObject> charactersInBattle; 
    public List<GameObject> livingPlayers;
    public List<GameObject> livingEnemies;
    public CharacterSheet currentCharacter;
    public GameObject targetedPlayer;
    public GameObject targetedEnemy;

    [Header("GUI Elements")]
    public Texture2D attackCursorTexture;
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
    [HideInInspector]
    public int characterListIndex = 0;
    public List<CharacterSheet> characterList;
    public CharacterSheet levelUpCharacter;
    public CharacterSheet defaultCharacter;
    public GameObject loseScreen;
    public GameObject winScreen;
    public GameObject battleHud;
    public GameObject openInventoryButton;
    public GameObject nextCharacterButton;
    public GameObject previousCharacterButton;
    public GameObject status;
    public GameObject tutorialMessage;
    
    private GameObject[] characterArray;
    public CursorOverlapCircle cursorOverlapCircle;
    private GameMaster gameMaster;
    private Pathfinding pathfinding;
    private CustomGrid grid;
    private PauseMenu pauseMenu;

    [HideInInspector]
    public List<PathNode> moveableNodes; //Must be public or becomes null
    [HideInInspector]
    public bool willWin = false;
    [HideInInspector]
    public bool attackDone = false;
    [HideInInspector]
    public bool turnOrderCalculated = false;
    [HideInInspector]
    public bool firstBattle = true;
    [HideInInspector]
    public bool showTutorial = true;
    [HideInInspector]
    public bool showTutorialPopups = true;
    [HideInInspector]
    public bool limitMovementDone = false;

    [Header("Inventory:")]
    public GameObject inventory;
    public InventoryUI inventoryUI;
    public EquipmentManager equipmentManager;
    public GameObject inventoryStatsText;
    public GameObject chestStatsText;
    [SerializeField]
    private Image equipmentPortrait;
    public GameObject chestMenu;
    public InventoryUI chestInventoryUI;
    public EquipmentManager chestEquipmentManager;
    [SerializeField]
    private Image chestEquipmentPortrait;
    [HideInInspector]
    public Inventory chest;
    public ChestUI chestContents; 

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
    [SerializeField]
    private GameObject levelUpNameText;
    #endregion
    #endregion

    private void Start()
    {
        if (battleStarted)
        {
            characterArray = GameObject.FindGameObjectsWithTag("Participant");
            charactersInBattle = new List<GameObject>(characterArray);
            StartBattle(charactersInBattle);
        }

        defaultCharacter = characterList[characterListIndex];
        gameMaster = GameMaster.instance.GetComponent<GameMaster>();
        pauseMenu = FindObjectOfType<PauseMenu>().GetComponent<PauseMenu>();
        pathfinding = FindObjectOfType<Pathfinding>().GetComponent<Pathfinding>();
        battleHud.SetActive(false);
        cursorOverlapCircle = FindObjectOfType<CursorOverlapCircle>().GetComponent<CursorOverlapCircle>();
        grid = FindObjectOfType<CustomGrid>().GetComponent<CustomGrid>();
        gameMaster.movedOnTurnEvent.AddListener(ResetMovementLimit);

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

            if (turnOrder.Count() < howFarInTheFutureYouCalculateTurnOrder)
            {
                CalculateTurnOrder();
            }

            //Displays the character to go's name on the screen
            turnText.text = "It is " + currentCharacter.Name + "'s turn";

            //Bandaid for sometimes when you click on an enemy really fast after battle starts some tiles remain blue even though you already moved on your turn 
            if (gameMaster.movedOnTurn)
            {
                ResetMovementLimit();
            }
        }
    }

    #region Battle Functions
    public void StartBattle(List<GameObject> participants)
    {        
        charactersInBattle = participants.ToList();
        battleStarted = true;
        battleHud.SetActive(true);
        battleHud.GetComponentInChildren<Animator>().SetTrigger("BattleStart");
        battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
        StartingTurnOrder();

        foreach (GameObject character in charactersInBattle)
        {
            if (character.GetComponent<CharacterSheet>().isPlayer)
            {
                livingPlayers.Add(character);
            }
        }

        foreach (GameObject character in charactersInBattle)
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

        //Display status of current character if they are a player
        if (currentCharacter.isPlayer)
        {
            currentCharacter.GetComponent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>());
        }

        if (!currentCharacter.isPlayer)
        {
            StartCoroutine(EnemyTurn());
        }

        //Add a delay so that participants can finish their move
        StartCoroutine(WaitHalfASecond());

        if (firstBattle && showTutorial)
        {
            firstBattle = false;
            DisplayTutorial();
        }
    }

    public void JoinBattle(GameObject newParticipant)
    {
        foreach (GameObject character in charactersInBattle)
        {
            character.GetComponent<CharacterSheet>().turnOrderPriority -= turnOrderPriorityReduction;
            if (character.GetComponent<CharacterSheet>().turnOrderPriority < 0)
            {
                character.GetComponent<CharacterSheet>().turnOrderPriority = 0;
            }
        }
        newParticipant.GetComponent<CharacterSheet>().turnOrderPriority -= turnOrderPriorityDisadvantage;
        charactersInBattle.Add(newParticipant); 
        livingEnemies.Add(newParticipant); 
        for (int i = 0; i < turnOrder.Count; i++) 
        {
            turnOrder.Clear();
        }
    }


    IEnumerator WaitHalfASecond()
    {
        yield return new WaitForSeconds(0.5f);
        LimitMovement();
    }

    public void LimitMovement()
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
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Shows blue tiles for where the player can get to
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            /* shows red tiles for where the enemy can get to
                            else
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.redTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            */
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
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Shows blue tiles for where the player can get to
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            /* shows red tiles for where the enemy can get to
                            else
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.redTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            */
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
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Shows blue tiles for where the player can get to
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            /* shows red tiles for where the enemy can get to
                            else
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.redTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            */
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
                        tempPath = pathfinding.FindPath(tempNode, oNode, maxMoveDistance);
                        if (tempPath == null)
                        {
                            continue;
                        }
                        foreach (PathNode node in tempPath)
                        {
                            moveableNodes.Add(node);
                            node.validMovePosition = true;
                            if (currentCharacter.GetComponent<CharacterSheet>().isPlayer) //Shows blue tiles for where the player can get to
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.blueTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            /* shows red tiles for where the enemy can get to
                            else
                            {
                                node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.redTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                            }
                            */
                        }
                    }
                }
            }
        }
        limitMovementDone = true;
    }

    public void ResetMovementLimit()
    {
        foreach (PathNode node in moveableNodes)
        {
            node.transform.GetChild(1).GetComponent<SpriteRenderer>().color = grid.whiteTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            node.validMovePosition = false;
        }
        moveableNodes.Clear();
    }

    private void DisplayTutorial()
    {
        tutorialMessage.SetActive(true);
        nextTurnButton.interactable = false;
    }

    public void CloseTutorial() //Called from button
    {
        showTutorial = false;
        nextTurnButton.interactable = true;
    }

    public void DisableTutorial() //Called from button
    {
        showTutorial = false;
        showTutorialPopups = false;
        gameMaster.hoveringOverButton = false;
    }

    public void NextTurn()
    {
        limitMovementDone = false;
        ResetMovementLimit();
        grid.gridClicked = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        currentCharacter = turnOrder[1].GetComponent<CharacterSheet>();
        attackDone = false;
        turnOrder.RemoveAt(0); 
        gameMaster.movedOnTurn = false;

        LimitMovement();
        LoadPortraits();
        gameMaster.GetComponent<CameraFollower>().SwitchFollow(currentCharacter.transform);

        if (!currentCharacter.isPlayer)
        {
            nextTurnButton.interactable = false;
            inventoryButton.interactable = false;
            StartCoroutine(EnemyTurn());
        }
        else
        {
            nextTurnButton.interactable = true;
            inventoryButton.interactable = true;
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
            foreach (GameObject character in charactersInBattle)
            {
                character.GetComponent<CharacterSheet>().turnOrderPriority += character.GetComponent<CharacterSheet>().characterStats.Reflexes;

                while (character.GetComponent<CharacterSheet>().turnOrderPriority >= multiTurnThreshold)
                {
                    character.GetComponent<CharacterSheet>().turnOrderPriority -= multiTurnThreshold;

                    turnOrder.Add(character);
                }
            }

            List<GameObject> tempList = turnOrder.Intersect(charactersInBattle).ToList();

            if (tempList.Count == charactersInBattle.Count && turnOrder.Count >= portraits.Count) //Once each character is in the list once AND there are enough characters in the turn order to fill all the portraits 
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
            foreach (GameObject character in charactersInBattle)
            {
                character.GetComponent<CharacterSheet>().turnOrderPriority += character.GetComponent<CharacterSheet>().characterStats.Reflexes;

                while (character.GetComponent<CharacterSheet>().turnOrderPriority >= multiTurnThreshold)
                {
                    character.GetComponent<CharacterSheet>().turnOrderPriority -= multiTurnThreshold;

                    turnOrder.Add(character);
                }
            }

            List<GameObject> tempList = turnOrder.Intersect(charactersInBattle).ToList();

            if (tempList.Count == charactersInBattle.Count)
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
            Movement movement = currentCharacter.GetComponentInParent<Movement>();
            if (closestPlayerPath == null && livingEnemies.Count > 0)
            {
                NextTurn();
            }
            else if (closestPlayerPath != null && closestPlayerPath.Count == 1) //If the player is right next to the enemy
            {
                //If the player is to the right of the enemy
                if (currentCharacter.transform.position.x - targetedPlayer.transform.position.x < 0)
                {
                    //If the enemy is facing left flip them
                    if (movement.spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if (movement.spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                //If the player is to the left or directly above/below the enemy
                if (currentCharacter.transform.position.x - targetedPlayer.transform.position.x >= 0)
                {
                    //If the enemy is facing right flip them
                    if (movement.spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else if (movement.spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                    {
                        currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                movement.attackAtEnd = false;
                currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                AudioManager.instance.Play(currentCharacter.attackSound);
            }
            else if (closestPlayerPath != null && closestPlayerPath.Count() != 0)
            {
                //Pathfind along that path until they are no longer validMovementNodes
                movement.endTurnAfterMove = true;
                movement.MoveOnPath(closestPlayerPath);
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
        Pathfinding pathfinding = gameMaster.GetComponent<Pathfinding>();
        int maxMoveDistance = currentCharacter.characterStats.Speed / 5;
        targetedPlayer = livingPlayers[Random.Range(0, livingPlayers.Count)]; //Randomly pick a player to attack
        bool tooFarAway = (Vector2.Distance(currentCharacter.GetComponentInParent<Movement>().occupyingNode.transform.position, gameMaster.partyNode.transform.position) > pathfinding.furthestAnyoneCanMove * giveUpDistance);
        if (!tooFarAway)
        {
            List<PathNode> shortestPath = pathfinding.FindPath(gameMaster.partyNode, currentCharacter.GetComponentInParent<Movement>().occupyingNode, maxMoveDistance);
            if (shortestPath == null)
            {
                //If the enemy cannot reach the player, check if they could if occupied nodes were stopping it 
                Debug.Log("Checking if occupied nodes stopped it");
                shortestPath = pathfinding.FindPath(gameMaster.partyNode, currentCharacter.GetComponentInParent<Movement>().occupyingNode, maxMoveDistance, true); //This makes the enemies intersect each other 
            }
            else if (shortestPath.Count > 0 && pathfinding.lastNodeRemoved == gameMaster.partyNode) //If the party is within the move range
            {
                currentCharacter.GetComponentInParent<Movement>().attackAtEnd = true;
            }
            return shortestPath;
        }
        Debug.Log("Removing current enemy, the player is too far away");
        livingEnemies.Remove(currentCharacter.gameObject);
        charactersInBattle.Remove(currentCharacter.gameObject);
        while (turnOrder.Contains(currentCharacter.gameObject))
        {
            turnOrder.Remove(currentCharacter.gameObject);
        }
        if (livingEnemies.Count == 0)
        {
            Reset();
            AudioManager.instance.Stop("BattleMusic");
            AudioManager.instance.Play("ExploringMusic");
        }
        return null;
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
            if (turnOrder[i].GetComponent<CharacterSheet>().isPlayer)
            {
                portraits[i].transform.parent.GetComponent<Image>().color = Color.white;
            }
            else
            {
                portraits[i].transform.parent.GetComponent<Image>().color = Color.red;
            }
        }
    }

    public void Attack()
    {
        //Don't let the player attack more than once per turn 
        if (attackDone)
        {
            return;
        }

        Cursor.SetCursor(attackCursorTexture, Vector2.zero, CursorMode.Auto);
    } //Called from button

    #endregion

    #region Inventory Functions
    public void OpenInventory()
    {
        inventoryOpen = true;
        pauseMenu.audioMixer.SetFloat("PausedMasterVolume", pauseMenu.amountQuieterWhenPaused);
        Time.timeScale = 0f;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        AudioManager.instance.Play("TurningPageInBookSound");
        inventory.SetActive(true);
        openInventoryButton.SetActive(false);
        levelUpButton.SetActive(false);
        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        equipmentManager.UpdateEquipmentUI(); //Updates inventory to match the right character

        if (!battleStarted)
        {
            while (defaultCharacter.isDead)
            {
                characterListIndex++;
                if (characterListIndex > characterList.Count - 1)
                {
                    characterListIndex = 0;
                }
                defaultCharacter = characterList[characterListIndex];
            }
            nextCharacterButton.SetActive(true);
            previousCharacterButton.SetActive(true);
            equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;

            inventoryStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
            inventoryStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
            inventoryStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
            inventoryStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
            inventoryStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
            inventoryStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
            inventoryStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
        }
        else if (currentCharacter.isPlayer)
        {
            battleHud.SetActive(false);
            nextCharacterButton.SetActive(false);
            previousCharacterButton.SetActive(false);
            equipmentPortrait.sprite = currentCharacter.GetComponent<SpriteRenderer>().sprite;

            inventoryStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + currentCharacter.characterStats.Strength;
            inventoryStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + currentCharacter.characterStats.Attunement;
            inventoryStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + currentCharacter.characterStats.Reflexes;
            inventoryStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + currentCharacter.characterStats.Speed;
            inventoryStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + currentCharacter.characterStats.Precision;
            inventoryStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + currentCharacter.characterStats.Constitution;
            inventoryStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + currentCharacter.characterStats.Endurance;
        }
    } 

    public void OpenChestMenu()
    {
        if (!chest.transform.GetChild(0).gameObject.activeSelf) //If the chest isn't locked
        {
            while (defaultCharacter.isDead)
            {
                characterListIndex++;
                if (characterListIndex > characterList.Count - 1)
                {
                    characterListIndex = 0;
                }
                defaultCharacter = characterList[characterListIndex];
            }

            inventoryOpen = true;
            pauseMenu.audioMixer.SetFloat("PausedMasterVolume", pauseMenu.amountQuieterWhenPaused);
            Time.timeScale = 0f;
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            gameMaster.hoveringOverButton = true;

            AudioManager.instance.Play("ChestOpen"); 
            chestMenu.SetActive(true);
            openInventoryButton.SetActive(false);
            levelUpButton.SetActive(false);
            chestInventoryUI.GetComponent<InventoryUI>().UpdateUI();
            chestEquipmentManager.UpdateEquipmentUI(); //Updates inventory to match the right character
            chestEquipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;

            chestContents.UpdateChestUI();

            chestStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
            chestStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
            chestStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
            chestStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
            chestStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
            chestStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
            chestStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
        }
    }

    public void CloseChestMenu()
    {
        inventoryOpen = false;
        pauseMenu.audioMixer.SetFloat("PausedMasterVolume", 0);
        Time.timeScale = 1f;
        AudioManager.instance.Play("ChestClose");
        for (int i = 0; i < characterList.Count - 1; i++)
        {
            if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp && !characterList[i].isDead)
            {
                levelUpButton.SetActive(true);
            }
        }
        openInventoryButton.SetActive(true);
        grid.gridClicked = false;
        gameMaster.hoveringOverButton = false;
        chestInventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        chestEquipmentManager.ClearEquipmentUI();
        chestMenu.SetActive(false);

        chestContents.ClearChestUI();
    } //Called from button

    public void CloseInventory()
    {
        inventoryOpen = false;
        pauseMenu.audioMixer.SetFloat("PausedMasterVolume", 0);
        Time.timeScale = 1f;
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
                if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp && !characterList[i].isDead)
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

        while (defaultCharacter.isDead)
        {
            characterListIndex++;
            if (characterListIndex > characterList.Count - 1)
            {
                characterListIndex = 0;
            }
            defaultCharacter = characterList[characterListIndex];
        }

        AudioManager.instance.Play("TurningPageInBookSound");
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        equipmentManager.ClearEquipmentUI();

        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        equipmentManager.UpdateEquipmentUI();

        inventoryStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
        inventoryStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
        inventoryStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
        inventoryStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
        inventoryStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
        inventoryStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
        inventoryStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
    } //Called from button
    public void PreviousCharacter() //remove buttons during battle
    {
        characterListIndex--;
        if (characterListIndex < 0)
        {
            characterListIndex = characterList.Count - 1;
        }
        defaultCharacter = characterList[characterListIndex];

        while (defaultCharacter.isDead)
        {
            characterListIndex++;
            if (characterListIndex > characterList.Count - 1)
            {
                characterListIndex = 0;
            }
            defaultCharacter = characterList[characterListIndex];
        }

        AudioManager.instance.Play("TurningPageInBookSound");
        inventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        equipmentManager.ClearEquipmentUI();

        inventoryUI.GetComponent<InventoryUI>().UpdateUI();
        equipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        equipmentManager.UpdateEquipmentUI();

        inventoryStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
        inventoryStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
        inventoryStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
        inventoryStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
        inventoryStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
        inventoryStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
        inventoryStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
    } //Called from button

    public void ChestNextCharacter() //remove buttons during battle
    {
        characterListIndex++;
        if (characterListIndex > characterList.Count - 1)
        {
            characterListIndex = 0;
        }
        defaultCharacter = characterList[characterListIndex];

        while (defaultCharacter.isDead)
        {
            characterListIndex++;
            if (characterListIndex > characterList.Count - 1)
            {
                characterListIndex = 0;
            }
            defaultCharacter = characterList[characterListIndex];
        }

        AudioManager.instance.Play("TurningPageInBookSound");
        chestInventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        chestEquipmentManager.ClearEquipmentUI();

        chestInventoryUI.GetComponent<InventoryUI>().UpdateUI();
        chestEquipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        chestEquipmentManager.UpdateEquipmentUI();

        chestStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
        chestStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
        chestStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
        chestStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
        chestStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
        chestStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
        chestStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
    } //Called from button
    public void ChestPreviousCharacter() //remove buttons during battle
    {
        characterListIndex--;
        if (characterListIndex < 0)
        {
            characterListIndex = characterList.Count - 1;
        }
        defaultCharacter = characterList[characterListIndex];

        while (defaultCharacter.isDead)
        {
            characterListIndex++;
            if (characterListIndex > characterList.Count - 1)
            {
                characterListIndex = 0;
            }
            defaultCharacter = characterList[characterListIndex];
        }

        AudioManager.instance.Play("TurningPageInBookSound");
        chestInventoryUI.GetComponent<InventoryUI>().ClearUI(); //Remove all items from inventory graphics
        chestEquipmentManager.ClearEquipmentUI();

        chestInventoryUI.GetComponent<InventoryUI>().UpdateUI();
        chestEquipmentPortrait.sprite = defaultCharacter.GetComponent<SpriteRenderer>().sprite;
        chestEquipmentManager.UpdateEquipmentUI();

        chestStatsText.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Strength " + defaultCharacter.characterStats.Strength;
        chestStatsText.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Attunement " + defaultCharacter.characterStats.Attunement;
        chestStatsText.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Reflexes " + defaultCharacter.characterStats.Reflexes;
        chestStatsText.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Speed " + defaultCharacter.characterStats.Speed;
        chestStatsText.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Precision " + defaultCharacter.characterStats.Precision;
        chestStatsText.transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Constitution " + defaultCharacter.characterStats.Constitution;
        chestStatsText.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Endurance " + defaultCharacter.characterStats.Endurance;
    } //Called from button

    #endregion

    #region LevelUp Functions
    public void LevelUp()
    {
        levelupScreenOpen = true;
        pauseMenu.audioMixer.SetFloat("PausedMasterVolume", pauseMenu.amountQuieterWhenPaused);
        Time.timeScale = 0f;
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
        levelUpNameText.GetComponent<TextMeshProUGUI>().text = levelUpCharacter.name;
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
        levelupScreenOpen = false;
        pauseMenu.audioMixer.SetFloat("PausedMasterVolume", 0);
        Time.timeScale = 1f;
        cursorOverlapCircle.EnableTutorialPopups();
        if (battleStarted)
        {
            battleHud.SetActive(true);
            battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", true);
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
        }
        else
        {
            for (int i = 0; i < characterList.Count(); i++)
            {
                if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp && !characterList[i].isDead)
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
            gameMaster.EndBattle();
            turnOrderCalculated = false;
            attackDone = false;
            turnOrder.Clear();
            charactersInBattle.Clear();
            livingPlayers.Clear();
            livingEnemies.Clear();
            ResetMovementLimit();
            nextTurnButton.interactable = true;
            inventoryButton.interactable = true;
            battleHud.GetComponentInChildren<Animator>().SetBool("BattleStarted", false);

            //At the end of battle, if anyone can level up, display the button for that character
            for (int i = 0; i < characterList.Count(); i++)
            {
                if (characterList[i].characterStats.XP >= characterList[i].characterStats.XPtoLevelUp)
                {
                    levelUpButton.SetActive(true);
                    levelUpCharacter = characterList[i];
                }
            }
        }
    }
}