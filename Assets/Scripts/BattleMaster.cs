using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattleMaster : MonoBehaviour
{
    public int multiTurnThreshold = 50;
    public Text turn;
    public List<GameObject> turnOrder = new();
    public Texture2D cursorTexture;
    public Button attackButton;

    private List<GameObject> tempList = new();
    private GameObject[] characterArray;

    public List<GameObject> characters;
    public List<GameObject> livingPlayers;
    public List<GameObject> livingEnemies;
    public GameObject currentCharacter;
    public bool attackPressed = false;
    public bool attackDone = false;

    private int characterindex = 0;
    private int turnCounter = 0;
    private bool battleStarted;

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
    }

    void Update()
    {
        if (attackDone)
        {
            attackButton.interactable = false;
        }

        //checks if the turn counter is closer than 20 from the furthest calculated the list has gone and if so calculates the list further
        if (turnCounter >= (turnOrder.Count() - 20))
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

        //listGenerator.Degenerate();

        //If the next person in line is not a player the ai will attack one of them at random
        if (!currentCharacter.GetComponent<CharacterSheet>().isPlayer)
        {
            GameObject target = livingPlayers[Random.Range(0, livingPlayers.Count())];
            currentCharacter.GetComponent<CharacterSheet>().Begin(); //Attack animation
            target.GetComponent<CharacterSheet>().TakeDamage(currentCharacter.GetComponent<CharacterSheet>().characterStats.Strength + 1);
            if (livingPlayers.Count() == 0)
            {
                //Display lose screen
            }
            NextTurn();
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
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            attackPressed = false;
            return;
        }

        attackPressed = true;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}
