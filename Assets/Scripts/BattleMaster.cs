using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BattleMaster : MonoBehaviour
{
    public int multiTurnThreshold = 50;
    public Text turn;
    public List<GameObject> turnOrder = new List<GameObject>();
    public Texture2D cursorTexture;

    private List<GameObject> tempList = new List<GameObject>();
    private List<GameObject> characters;
    private GameObject[] characterArray;

    //[HideInInspector]
    public GameObject currentCharacter;
    public bool attackPressed = false;
    public bool attackDone = false;

    private AttackListGenerator listGenerator;
    private int characterindex = 0;
    private int turnCounter = 0;
    private bool battleStarted;
    private bool turnPassed;

    void Start()
    {
        //Finds all the participants
        characterArray = GameObject.FindGameObjectsWithTag("Participant");
        characters = new List<GameObject>(characterArray);

        listGenerator = GetComponent<AttackListGenerator>();

        StartingTurnOrder();

        turnPassed = false;
        battleStarted = true;
        currentCharacter = turnOrder[0];
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
        //turnpassed will be changed in a unity event at the end of the current characters turn
        if(turnPassed)
        {
            characterindex++;
            currentCharacter = turnOrder[characterindex];
            turnPassed = false;
        }

        //checks to ensure that there is not already a list created and if not, creates one
        /*if (!generated)
        {
            if (!currentCharacter.GetComponent<Enemy>())
            {
                listGenerator.Generate(currentCharacter.GetComponentInChildren<Canvas>().gameObject);
                generated = true;
            }
        }*/

        //checks when reached the end of the current order and makes a new one. Recalculated at -1 to avoid index errors
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
        attackDone = false;
        turnCounter++;

        listGenerator.Degenerate();
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
        if (attackDone)
        {
            return;
        }
        if (attackPressed)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            attackPressed = false;
            return;
        }
        attackPressed = true;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        //Play attack animation
        //Grey out the attack button when it can't be used
    }
}
