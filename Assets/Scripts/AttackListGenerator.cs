using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirFishLab.ScrollingList;

public class AttackListGenerator : MonoBehaviour
{
    public GameObject playerCanvas;

    public List<string> names;

    public void Generate()
    {
        //names = GetComponent<CharacterSheet>().characterAttacks.attackNames;

        GameObject attackList = Instantiate(Resources.Load("Attack List") as GameObject, playerCanvas.transform.position, playerCanvas.transform.rotation, playerCanvas.transform);
        CircularScrollingList UIList = attackList.GetComponent<CircularScrollingList>();

        attackList.GetComponent<RectTransform>().anchorMin = new Vector2(.5f, 0f);
        attackList.GetComponent<RectTransform>().anchorMax = new Vector2(.5f, 0f);
        attackList.GetComponent<RectTransform>().pivot = new Vector2(.5f, 0f);

        foreach (string name in names)
        {
            GameObject newButton = Instantiate(Resources.Load(name) as GameObject, playerCanvas.transform.position, playerCanvas.transform.rotation, attackList.transform);
            UIList._listBoxes.Add(newButton.GetComponent<PlayerListBox>());
            UIList.GetComponent<PlayerListBank>().attackNames.Add(newButton.name);
        }

        UIList.Initialize();
    }
}
