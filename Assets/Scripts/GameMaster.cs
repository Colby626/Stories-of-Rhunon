using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public int battleBeginRange;
    public int distanceToLookForParticipants;
    public List<GameObject> participants;

    public GameObject battleMaster;
    public GameObject battleHud;
    public GameObject menus;
    public CustomGrid grid;
    public PathNode partyNode;
    public PathNode targetNode;

    private RaycastHit2D[] hits;

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
        //If enemies in battleBeginRange
        //battlestart
        //lookforparticipants
    }

    public void LookForParticipants()
    {
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        hits = Physics2D.BoxCastAll(new Vector2(partyNode.x, partyNode.y), new Vector2(distanceToLookForParticipants, distanceToLookForParticipants), 0, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.transform.gameObject.CompareTag("Participant"))
            {
                participants.Append(hit.transform.gameObject);
            }
        }
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
}
