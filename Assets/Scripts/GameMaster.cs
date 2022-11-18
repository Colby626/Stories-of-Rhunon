using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;
    public Material highlightMaterial;

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
        //Starts battle when an enemy is within one square of the players 
        //Will be changed to when an enemy is within viewing range of the player 
        //The Look for participants will be centered on the enemy that saw the player
        //It will also search from the center of any enemy that is within the range of the first one 
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
                if (collider.GetComponent<CharacterSheet>().isPlayer)
                {
                    collider.gameObject.AddComponent<MouseOver>();
                    collider.GetComponent<MouseOver>().battleMaster = battleMaster.GetComponent<BattleMaster>();
                    collider.GetComponent<MouseOver>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().manaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
                    collider.GetComponent<MouseOver>().portrait = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                    collider.GetComponent<MouseOver>().nameText = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(4).GetComponent<Text>();
                    collider.GetComponent<MouseOver>().overheadNameText = collider.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
                    collider.GetComponent<MouseOver>().highlightMaterial = highlightMaterial;
                }
                else
                {
                    collider.transform.parent.gameObject.AddComponent<MouseOver>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().battleMaster = battleMaster.GetComponent<BattleMaster>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().overheadHealthBar = collider.transform.GetChild(0).GetChild(0).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().healthBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(1).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().manaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(2).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().staminaBar = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(3).GetComponent<StatBar>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().portrait = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().nameText = battleMaster.GetComponent<BattleMaster>().status.transform.GetChild(4).GetComponent<Text>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().overheadNameText = collider.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
                    collider.transform.parent.gameObject.GetComponent<MouseOver>().highlightMaterial = highlightMaterial;
                }
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
        foreach (GameObject participant in participants)
        {
            if (participant.GetComponentInParent<Movement>().vectorPath.Count > 0)
            {
                Vector3 finishMove = participant.GetComponentInParent<Movement>().vectorPath[0];
                participant.GetComponentInParent<Movement>().vectorPath.Clear();
                participant.GetComponentInParent<Movement>().vectorPath.Add(finishMove);
            }
        }
        AudioManager.instance.Stop("ExploringMusic");
        AudioManager.instance.Play("BattleMusic");
        battleMaster.GetComponent<BattleMaster>().StartBattle(participants);
    }

    public void EndBattle()
    {
        //Set movement active again
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
