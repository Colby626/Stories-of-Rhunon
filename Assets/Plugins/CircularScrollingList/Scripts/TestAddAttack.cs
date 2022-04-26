using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirFishLab.ScrollingList;

public class TestAddAttack : MonoBehaviour
{
    private GameObject attackList;
    private CircularScrollingList uiList;

    private void Start()
    {
        attackList = GameObject.Find("Attack List");
        uiList = attackList.GetComponent<CircularScrollingList>();
    }


    public void AddNewAttack()
    {
        //GameObject newAttack = Instantiate((GameObject)Resources.Load("Attack"), attackList.transform.position, attackList.transform.rotation);
        //newAttack.transform.parent = attackList.transform;
        //uiList._listBoxes.Add(newAttack.GetComponent<PlayerListBox>());
        //attackList.GetComponent<PlayerListBank>().attackNames.Add(newAttack.name);
        //uiList.Refresh();
    }
}
