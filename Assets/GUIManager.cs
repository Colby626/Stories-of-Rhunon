using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    public GameObject attackOptions;
    public BattleMaster BM;
    private string[] attacks;


    public void DisplayAttacks()
    {
        attacks = BM.nextCharacter.GetComponent<CharacterSheet>().attackNames;

        foreach (string text in attacks)
        {
            if (text == attackOptions.GetComponent<TextMeshProUGUI>().text)
            {
                attackOptions.SetActive(true);
            }
        }
    }
}
