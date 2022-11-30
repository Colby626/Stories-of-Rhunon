using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public int distanceToLookForParticipants;
    public List<GameObject> participants;
    public Material highlightMaterial;

    public GameObject party;
    public BattleMaster battleMaster;
    public CustomGrid grid;
    public PathNode partyNode;
    public PathNode targetNode;
    public bool startPositionDetermined = false;
    public bool movedOnTurn = false;
    public bool hoveringOverButton;

    private Collider2D[] colliders;

    public static GameMaster instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else //On new scene load when there is already a GameMaster
        {
            instance.grid = FindObjectOfType<CustomGrid>();
            instance.battleMaster = FindObjectOfType<BattleMaster>();
            instance.party = GameObject.FindGameObjectWithTag("Party");
            instance.hoveringOverButton = false;
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);

        instance.grid = FindObjectOfType<CustomGrid>();
        instance.battleMaster = FindObjectOfType<BattleMaster>();
        instance.party = GameObject.FindGameObjectWithTag("Party");
    }

    private void Update()
    {
        if (party != null)
        {
            party.GetComponent<Movement>().occupyingNode = partyNode;
        }
    }

    public void LookForParticipants(GameObject caller)
    {
        if (participants.Contains(caller))
        {
            return;
        }
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        colliders = Physics2D.OverlapBoxAll(caller.transform.position, new Vector2(distanceToLookForParticipants, distanceToLookForParticipants), 0);

        foreach (Collider2D collider in colliders) //This should be redone for optimization and just set in the editor 
        {
            if (collider.transform.gameObject.CompareTag("Participant"))
            {
                participants.Add(collider.gameObject);

                if (!collider.GetComponent<CharacterSheet>().isPlayer)
                {
                    LookForParticipants(collider.gameObject);
                    collider.GetComponentInParent<Movement>().lookingForParticipants = true;
                }
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
        partyNode = null;
        startPositionDetermined = false;
        movedOnTurn = false;
        participants.Clear();
        hoveringOverButton = false;
    }
}