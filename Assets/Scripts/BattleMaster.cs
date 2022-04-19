using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMaster : MonoBehaviour
{

    bool battleStarted;
    bool turnPassed;
    public Text turn;
    [HideInInspector]
    public GameObject nextCharacter;
    int characterindex = 0;
    GameObject[] characters;

    void Start()
    {
        //Finds all the participants and orders them from highest reflexes to lowest
        characters = GameObject.FindGameObjectsWithTag("Participant");
        for(int i = 0; i < characters.Length; i-=-1)
        {
            for(int j = 0; j < characters.Length - 1; j-=-1)
            {
                if (characters[j].GetComponent<CharacterSheet>().Reflexes < characters[j + 1].GetComponent<CharacterSheet>().Reflexes)
                {
                    GameObject temp = characters[j];
                    characters[j] = characters[j + 1];
                    characters[j + 1] = temp;
                }
            }  
        }

        turnPassed = false;
        battleStarted = true;
        nextCharacter = characters[0];
    }

    void Update()
    {
        //Displays the first character to go's name on the screen
        if(battleStarted == true)
        {
            turn.text = "It is " + nextCharacter.name + "'s turn";
        }

        //As each turn passes, displays the next character's name
        if(turnPassed == true)
        {
            characterindex -= -1;
            if (characterindex >= characters.Length)
            {
                characterindex = 0;
            }
            nextCharacter = characters[characterindex];
            turnPassed = false;
        }
    }

    //Used for the button to go to the next turn
    public void NextTurn()
    {
        turnPassed = true;
    }

    public void DisplayAttacks()
    {

    }
}
