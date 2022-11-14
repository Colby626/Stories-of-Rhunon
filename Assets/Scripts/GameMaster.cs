using System.Collections.Generic;
using System.Linq;
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
    public bool startPositionDetermined = false;

    private bool battleSetupStarted = false;
    private Collider2D[] colliders;
    private List<PathNode> partyNeighbors = new();

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

    private void Update()
    {
        if (startPositionDetermined && !battleSetupStarted)
        {
            partyNeighbors = partyNode.GetNeighborNodes();
        
            foreach (PathNode node in partyNeighbors)
            {
                if (node.transform.GetChild(1).GetComponent<SpriteRenderer>().color != node.GetComponent<PathNode>().baseColor)
                {
                    battleSetupStarted = true;
                    LookForParticipants();
                }
            }
        }
    }

    public void LookForParticipants()
    {
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        colliders = Physics2D.OverlapBoxAll(new Vector2(partyNode.x + grid.origin.x, partyNode.y + grid.origin.y), new Vector2(distanceToLookForParticipants, distanceToLookForParticipants), 0);

        foreach (Collider2D collider in colliders)
        {
            if (collider.transform.gameObject.CompareTag("Participant"))
            {
                participants.Add(collider.transform.gameObject);
            }
        }

        StartBattle();
    }

    public void StartBattle()
    {
        //Set Mouseover and Status Manager to active on the players
        //Instantiate(battleHud);
        //Instantiate(menus);
    }

    public void EndBattle()
    {
        participants.Clear();
    }

    private void OnDrawGizmos()
    {
        if (battleSetupStarted)
        {
            Gizmos.color = Color.yellow; //Sets the color of the distance it looks for participants to yellow
            Gizmos.DrawWireCube(new Vector3(partyNode.x + grid.origin.x, partyNode.y + grid.origin.y), new Vector3(distanceToLookForParticipants, distanceToLookForParticipants, 0)); //Display for how far distance to look for participants is
        }
    }
}
