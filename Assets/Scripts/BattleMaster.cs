using System.Collections.Generic; //For lists
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro; //For name text under turn order portraits

public class BattleMaster : MonoBehaviour
{
    public int multiTurnThreshold = 50;
    public Text turn;
    public List<GameObject> turnOrder = new();
    public Texture2D cursorTexture;
    public Button attackButton;

    private List<GameObject> tempList = new();
    private GameObject[] characterArray;
    public List<Image> portraits;
    public List<TextMeshProUGUI> namesList;
    public List<StatBar> healthBars;
    public List<GameObject> characters;
    public List<GameObject> livingPlayers;
    public List<GameObject> livingEnemies;
    public GameObject currentCharacter;
    public GameObject loseScreen;
    public GameObject winScreen;
    public GameObject battleHud;
    [HideInInspector]
    public bool attackPressed = false;
    [HideInInspector]
    public bool attackDone = false;
    [HideInInspector]
    public bool battleStarted;
    public int howFarInTheFutureYouCalculateTurnOrder = 50;

    private int characterindex = 0;
    private int turnCounter = 0;

    [Header("Inventory:")]
    public GameObject inventory;
    public InventoryUI inventoryUI;
    public Image equipmentPortrait;

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


    void Start()
    {
        //Finds all the participants
        characterArray = GameObject.FindGameObjectsWithTag("Participant");
        characters = new List<GameObject>(characterArray);

        //Gets a list of the players
        foreach (GameObject character in characterArray)
        {
            if (character.GetComponent<CharacterSheet>().isPlayer)
            {
                livingPlayers.Add(character);
            }
        }

        //Gets a list of the enemies
        foreach (GameObject character in characterArray)
        {
            if (!character.GetComponent<CharacterSheet>().isPlayer)
            {
                livingEnemies.Add(character);
            }
        }
        StartingTurnOrder();

        battleStarted = true;
        currentCharacter = turnOrder[0];

        //Display status of current character
        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            currentCharacter.GetComponent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>());
        }

        //Display portraits, names, and healths for the turn order
        for (; turnCounter < portraits.Count(); turnCounter++)
        {
            portraits[turnCounter].sprite = turnOrder[turnCounter].GetComponent<CharacterSheet>().Portrait;
            namesList[turnCounter].text = turnOrder[turnCounter].GetComponent<CharacterSheet>().Name;
            healthBars[turnCounter].SetBarMax(turnOrder[turnCounter].GetComponent<CharacterSheet>().MaxHealth);
            healthBars[turnCounter].SetBar(turnOrder[turnCounter].GetComponent<CharacterSheet>().Health);
        }
        turnCounter -= portraits.Count();

        //Displays the levelUpButton if the currentCharacter has enough XP to LevelUp
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
    }

    void Update()
    {
        if (attackDone)
        {
            attackButton.interactable = false;
        }

        //checks if the turn counter is closer than 20 from the furthest calculated the list has gone and if so calculates the list further
        if (turnCounter >= (turnOrder.Count() - howFarInTheFutureYouCalculateTurnOrder))
        {
            CalculateTurnOrder();
        }

        //Displays the character to go's name on the screen
        //will be removed
        if (battleStarted)
        {
            turn.text = "It is " + currentCharacter.GetComponent<CharacterSheet>().Name + "'s turn";
        }
    }

    //Used for the button to go to the next turn
    public void NextTurn()
    {
        characterindex++;
        currentCharacter = turnOrder[characterindex];
        attackDone = false;
        turnCounter++;
        attackButton.interactable = true;

        //Display portraits, names, and healths for the turn order
        for (int i = 0; i < portraits.Count(); i++)
        {
            portraits[i].sprite = turnOrder[turnCounter+i].GetComponent<CharacterSheet>().Portrait;
            namesList[i].text = turnOrder[turnCounter+i].GetComponent<CharacterSheet>().Name;
            healthBars[i].SetBarMax(turnOrder[turnCounter+i].GetComponent<CharacterSheet>().MaxHealth);
            healthBars[i].SetBar(turnOrder[turnCounter+i].GetComponent<CharacterSheet>().Health);
        }

        //If the next person in line is not a player the ai will attack one of them at random
        if (!currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            GameObject target = livingPlayers[Random.Range(0, livingPlayers.Count())];
            currentCharacter.GetComponent<CharacterSheet>().Begin(); //Attack animation
            target.GetComponent<CharacterSheet>().TakeDamage(currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength + 1);

            //If there are no more players alive, display the lose screen
            if (livingPlayers.Count() == 0)
            {
                battleStarted = false;
                battleHud.SetActive(false);
                loseScreen.SetActive(true);
            }
            else
            {
                NextTurn();
            }
        }

        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }

        if (currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            currentCharacter.GetComponent<MouseOver>().ActivateStatus(currentCharacter.GetComponent<CharacterSheet>()); //Activates the status menu at the bottom to match the current character
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

            if (tempList.Count == characters.Count)
            {
                exit = true;
            }
        }
    }

    public void Attack()
    {
        //Don't let them attack more than once per turn 
        if (attackDone)
        {
            return;
        }

        //If they are pressing the attack button after they already pressed it 
        if (attackPressed)
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            attackPressed = false;
            return;
        }

        attackPressed = true;
        UnityEngine.Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OpenInventory()
    {
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
        battleHud.SetActive(true);
        //Remove all items from inventory graphics
        inventoryUI.GetComponent<InventoryUI>().ClearUI();
        GameObject.FindGameObjectWithTag("EquipmentManager").GetComponent<EquipmentManager>().ClearEquipmentUI();
        inventory.SetActive(false);
    }

    public void LevelUp()
    {
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
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpAttunement()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Attunement += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpReflexes()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Reflexes += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpSpeed()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Speed += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpPrecision()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Precision += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpConstitution()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Constitution += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }

    public void LevelUpEndurance()
    {
        currentCharacter.GetComponent<CharacterSheet>().characterStats.Endurance += 1;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XP -= currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp;
        currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp += 10; //It requires 10 more xp per levelup to level up again
        battleHud.SetActive(true);
        //Display the levelup button if the currentCharacter has more XP than they need to level up
        if (currentCharacter.GetComponent<CharacterSheet>().characterStats.XP > currentCharacter.GetComponent<CharacterSheet>().characterStats.XPtoLevelUp)
        {
            levelUpButton.SetActive(true);
        }
        else
        {
            levelUpButton.SetActive(false);
        }
        levelUpPanel.SetActive(false);
    }
    #endregion
}
