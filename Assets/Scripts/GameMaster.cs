using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;

    public GameObject battleMaster;
    public GameObject battleHud;
    public GameObject menus;
    public CustomGrid grid;
    public PathNode partyNode;
    public PathNode targetNode;

    public static GameMaster instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void LookForParticipants()
    {
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
    }

    public void StartBattle(List<GameObject> battlers)
    {
        participants = battlers;
        //Set Mouseover and Status Manager to active on the players
        //Instantiate(battleHud);
        //Instantiate(menus);
    }

    public void EndBattle()
    {
        participants.Clear();
    }
}
