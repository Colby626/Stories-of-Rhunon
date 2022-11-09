using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.UI.Image;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;

    public GameObject battleMaster;
    public GameObject battleHud;
    public GameObject menus;
    public CustomGrid grid;
    public GameObject Party;
    public int partyX;
    public int partyY;

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

    private void Start()
    {
        Party.transform.position = new Vector3(grid.origin.x + partyX + 0.5f, grid.origin.y + partyY + 0.5f, 0);
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
