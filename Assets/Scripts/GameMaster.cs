using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public Material highlightMaterial;

    public PathNode partyNode;
    public PathNode targetNode;
    public bool startPositionDetermined = false;
    public bool movedOnTurn = false;
    public bool hoveringOverButton;
    public List<GameObject> participants;

    [HideInInspector]
    public GameObject party;
    [HideInInspector]
    public BattleMaster battleMaster;
    [HideInInspector]
    public CustomGrid grid;

    private float gridTimer = 0.1f;
    private float gridTime;
    private bool timerSet = false;
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
            instance.participants.Clear();
            for (int i = 0; i < participants.Count; i++)
            {
                instance.participants.RemoveAt(0);
            }
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

        if (grid.gridClicked && !timerSet)
        {
            gridTime = gridTimer;
            timerSet = true;
        }

        gridTime -= Time.deltaTime;
        if (gridTime <= 0.0f)
        {
            grid.gridClicked = false;
            timerSet = false;
        }
    }

    public void LookForParticipants(GameObject caller)
    {
        if (participants.Contains(caller))
        {
            return;
        }
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        colliders = Physics2D.OverlapBoxAll(caller.transform.position, new Vector2(caller.GetComponentInParent<Movement>().distanceToLookForParticipants, caller.GetComponentInParent<Movement>().distanceToLookForParticipants), 0);

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

    private void StartBattle()
    {
        foreach (GameObject participant in participants)
        {
            if (participant.GetComponentInParent<Movement>().vectorPath.Count > 0)
            {
                Vector3 finishMove = participant.GetComponentInParent<Movement>().vectorPath[0];
                participant.GetComponentInParent<Movement>().vectorPath.Clear();
                participant.GetComponentInParent<Movement>().vectorPath.Add(finishMove);
                //Random -1 on next line is required to match with where they end up, I don't know why it is needed
                participant.GetComponentInParent<Movement>().startingNode = grid.GetGridObject((int)participant.GetComponentInParent<Movement>().vectorPath[0].x - grid.origin.x - 1, (int)participant.GetComponentInParent<Movement>().vectorPath[0].y - grid.origin.y);
            }
            participant.GetComponentInParent<Movement>().lookingForParticipants = false;
        }
        AudioManager.instance.Stop("ExploringMusic");
        AudioManager.instance.Play("BattleMusic");
        battleMaster.StartBattle(participants);
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