using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;

    public GameObject party;
    public GameObject battleMaster;
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
                if (node.transform.GetChild(1).GetComponent<SpriteRenderer>().color != node.GetComponent<PathNode>().baseColor) //Start battle sequence if next to a tile that has a different color than base color
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
                collider.GetComponentInParent<Movement>().wander = false;
                collider.gameObject.AddComponent<MouseOver>();
                collider.gameObject.GetComponent<MouseOver>().battleMaster = battleMaster.GetComponent<BattleMaster>();
                collider.gameObject.GetComponent<MouseOver>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                collider.gameObject.GetComponent<MouseOver>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                collider.gameObject.GetComponent<MouseOver>().manaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                collider.gameObject.GetComponent<MouseOver>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
                collider.gameObject.GetComponent<MouseOver>().portrait = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                collider.gameObject.GetComponent<MouseOver>().nameText = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(4).GetComponent<Text>();
                collider.gameObject.AddComponent<StatusManager>();
                collider.gameObject.GetComponent<StatusManager>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().magicBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                collider.gameObject.GetComponent<StatusManager>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
            }
        }

        StartBattle();
    }

    public void StartBattle()
    {
        party.GetComponent<Movement>().enabled = false;
        AudioManager.instance.Stop("ExploringMusic");
        AudioManager.instance.Play("BattleMusic");
        battleMaster.GetComponent<BattleMaster>().StartBattle(participants);
    }

    public void EndBattle()
    {
        party.GetComponent<Movement>().enabled = true;
        AudioManager.instance.Stop("BattleMusic");
        AudioManager.instance.Play("ExploringMusic");
        battleMaster.GetComponent<BattleMaster>().Reset();
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
