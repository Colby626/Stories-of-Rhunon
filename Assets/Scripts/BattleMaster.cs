using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattleMaster : MonoBehaviour
{
    public int multiTurnThreshold = 50;
    public Text turn;
    public List<GameObject> turnOrder = new List<GameObject>();

    private List<GameObject> tempList = new List<GameObject>();
    private List<GameObject> characters;
    private GameObject[] characterArray;
    //[HideInInspector]
    public GameObject currentCharacter;
    private AttackListGenerator listGenerator;
    private int characterindex = 0;
    private int turnCounter = 0;
    private bool battleStarted;
    private bool turnPassed;

    void Start()
    {
        //Finds all the participants and orders them from highest reflexes to lowest
        characterArray = GameObject.FindGameObjectsWithTag("Participant");
        characters = new List<GameObject>(characterArray);

        listGenerator = GetComponent<AttackListGenerator>();

        StartingTurnOrder();

        turnPassed = false;
        battleStarted = true;
        currentCharacter = turnOrder[0];

        //generate first list of options at first character
    }

    void Update()
    {
        //Displays the first character to go's name on the screen
        //will be removed
        if(battleStarted)
        {
            turn.text = "It is " + currentCharacter.GetComponent<CharacterSheet>().Name + "'s turn";
        }

        //As each turn passes, displays the next character's name
        //turnpassed will be called in a unity event at the end of the current characters turn
        if(turnPassed)
        {
            characterindex++;
            currentCharacter = turnOrder[characterindex];
            turnPassed = false;
        }

        if (turnCounter == (turnOrder.Count - 1))
        {
            CalculateTurnOrder();

            //update UI turn order portraits

            turnCounter = 0;
            characterindex = 0;
            currentCharacter = turnOrder[0];
        }
    }

    //Used for the button to go to the next turn
    public void NextTurn()
    {
        turnPassed = true;
        turnCounter++;
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

    //Used for the attack button to deal damage
    //public void Attack(GameObject target)
    //{
    //    //Make the movement happen over time
    //    //Have a set target be highlighted by a mousepress
    //    //Make the button only work once per turn
    //    int damage = nextCharacter.GetComponent<CharacterSheet>().characterStats.Strength + 1;
    //    Vector2 startPosition = nextCharacter.transform.position;
    //    Vector2 targetPosition = target.gameObject.transform.position;
    //    nextCharacter.transform.position = Vector2.MoveTowards(startPosition, targetPosition, nextCharacter.GetComponent<CharacterSheet>().movementSpeed*Time.deltaTime);
    //    target.GetComponent<CharacterSheet>().TakeDamage(damage);
    //    nextCharacter.transform.position = Vector2.MoveTowards(nextCharacter.transform.position, startPosition, nextCharacter.GetComponent<CharacterSheet>().movementSpeed*Time.deltaTime);
    //}
}
