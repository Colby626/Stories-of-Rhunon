using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMaster : MonoBehaviour
{
    public Material highlightMaterial;

    public PathNode partyNode;
    public PathNode targetNode;
    public bool movedOnTurn = false;
    public UnityEvent movedOnTurnEvent;
    public bool hoveringOverButton;
    public bool waitingOnEveryoneToStop = false;
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
    private bool noneMoving = true;
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
            instance.GetComponent<Pathfinding>().gameMaster = instance;
            instance.GetComponent<Pathfinding>().grid = instance.grid;
            instance.GetComponent<CameraFollower>().camera = FindObjectOfType<CinemachineVirtualCamera>();
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
        GetComponent<Pathfinding>().gameMaster = instance;
        GetComponent<Pathfinding>().grid = instance.grid;
    }

    private void Update()
    {
        if (waitingOnEveryoneToStop)
        {
            noneMoving = true;
            foreach (GameObject participant in participants)
            {
                if (participant.GetComponentInParent<Movement>().isMoving)
                {
                    noneMoving = false;
                }
            }
            if (noneMoving)
            {
                waitingOnEveryoneToStop = false;
            }
        }

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
            if (collider.GetComponent<CharacterSheet>())
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

    public void JoinBattle(GameObject caller)
    {
        if (participants.Contains(caller) || Vector2.Distance(battleMaster.currentCharacter.GetComponentInParent<Movement>().occupyingNode.transform.position, partyNode.transform.position) > GetComponent<Pathfinding>().furthestAnyoneCanMove * 2)
        {
            return;
        }
        //check tiles outward from contact with enemy and player with a distance of distanceToLookForPartcipants and put them in participants list then call StartBattle
        colliders = Physics2D.OverlapBoxAll(caller.transform.position, new Vector2(caller.GetComponentInParent<Movement>().distanceToLookForParticipants, caller.GetComponentInParent<Movement>().distanceToLookForParticipants), 0);

        foreach (Collider2D collider in colliders) //This should be redone for optimization and just set in the editor 
        {
            if (collider.GetComponent<CharacterSheet>())
            {
                if (participants.Contains(collider.gameObject))
                {
                    continue;
                }

                if (!collider.GetComponent<CharacterSheet>().isPlayer)
                {
                    participants.Add(collider.gameObject); 
                    battleMaster.JoinBattle(collider.gameObject);
                    JoinBattle(collider.gameObject);
                    collider.GetComponentInParent<Movement>().lookingForParticipants = true;
                }
            }
        }
    }

    private void StartBattle()
    {
        foreach (GameObject participant in participants)
        {
            Movement movement = participant.GetComponentInParent<Movement>();
            if (movement.vectorPath.Count > 0)
            {
                Vector3 finishMove = movement.vectorPath[0];
                movement.vectorPath.Clear();
                movement.vectorPath.Add(finishMove);
                //Random -1 on next line is required to match with where they end up, I don't know why it is needed
                movement.startingNode = grid.GetGridObject((int)movement.vectorPath[0].x - grid.origin.x - 1, (int)movement.vectorPath[0].y - grid.origin.y);
            }
            movement.lookingForParticipants = false;
            if (movement.isMoving)
            {
                noneMoving = false;
            }
        }
        if (!noneMoving)
        {
            waitingOnEveryoneToStop = true;
        }
        AudioManager.instance.Stop("ExploringMusic");
        AudioManager.instance.Play("BattleMusic");
        battleMaster.StartBattle(participants);
    }

    public void EndBattle()
    {
        //Set movement active again
        partyNode = null;
        movedOnTurn = false;
        participants.Clear();
        hoveringOverButton = false;
        GetComponent<CameraFollower>().SwitchFollow(party.transform);
    }
}