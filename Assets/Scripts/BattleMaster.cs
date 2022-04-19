using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleMaster : MonoBehaviour
{

    bool battleStarted;
    bool turnPassed;
    public Text turn;
    GameObject nextCharacter;
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
                if (characters[j].GetComponent<CharacterSheet>().characterStats.Reflexes < characters[j + 1].GetComponent<CharacterSheet>().characterStats.Reflexes)
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
            turn.text = "It is " + nextCharacter.gameObject.name + "'s turn";
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

    //Used for the attack button to deal damage
    public void Attack(GameObject target)
    {
        //Make the movement happen over time
        //Have a set target be highlighted by a mousepress
        //Make the button only work once per turn
        int damage = nextCharacter.GetComponent<CharacterSheet>().characterStats.Strength + 1;
        Vector2 startPosition = nextCharacter.transform.position;
        Vector2 targetPosition = target.gameObject.transform.position;
        nextCharacter.transform.position = Vector2.MoveTowards(startPosition, targetPosition, nextCharacter.GetComponent<CharacterSheet>().movementSpeed*Time.deltaTime);
        target.GetComponent<CharacterSheet>().TakeDamage(damage);
        nextCharacter.transform.position = Vector2.MoveTowards(nextCharacter.transform.position, startPosition, nextCharacter.GetComponent<CharacterSheet>().movementSpeed*Time.deltaTime);
    }
}
