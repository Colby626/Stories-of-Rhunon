using System.Collections.Generic; //For lists
using UnityEngine;

public class Movement : MonoBehaviour //Base Movement class that certain enemy AIs can derive from
{
    public bool isPlayer;
    [Tooltip("How close to the center of a tile an entity must be to consider itself in that tile for movement")]
    public float centeringOffset = 0.1f;
    public bool spriteFacingLeft;
    public PathNode occupyingNode;

    [Header("Enemy Stuff:")]
    [Tooltip("Blue circle")]
    public int viewRange;
    [Tooltip("Red circle")]
    public int wanderRange;
    public bool wander = false;
    public float wanderDelayMin;
    public float wanderDelayMax;
    public RaycastHit2D[] visibleRange;
    public bool lookingForParticipants = false;
    [HideInInspector]
    public bool attackAtEnd = false;

    private GameMaster gameMaster;
    private Pathfinding pathfinding;
    private CustomGrid grid;
    private PathNode startingNode;
    private List<PathNode> playerPath;
    private RaycastHit2D[] raycast;
    private Collider2D[] colliders;
    public List<Vector2> vectorPath;
    private Vector2 targetPosition;
    private Vector2 moveDirection;
    private bool isMoving = false;
    private bool hasBeenMoving = false;
    private bool startPositionDetermined = false;

    private List<PathNode> wanderNodes;
    private PathNode wanderNode;
    private bool wanderSetup = false;
    private float wanderTimer;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        grid = gameMaster.GetComponent<GameMaster>().grid;
        pathfinding = gameMaster.GetComponent<Pathfinding>();
        vectorPath = new List<Vector2>(); //Prevents an error in the console
    }

    private void DetermineStartPosition()
    {
        //Set the startingNode from wherever they are 
        colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.5f, 0.5f), 0);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetComponent<PathNode>() == true)
            {
                startingNode = colliders[i].GetComponent<PathNode>();
                break;
            }
        }

        if (startingNode == null)
        {
            Debug.LogError("Starting position not determined");
        }

        if (isPlayer)
        {
            gameMaster.partyNode = startingNode;
        }
    }

    private void Update()
    {
        if (grid.gridFinished && !startPositionDetermined)
        {
            startPositionDetermined = true;
            gameMaster.startPositionDetermined = true;
            DetermineStartPosition();
        }
        if (grid.gridFinished && !wanderSetup && wander && !isPlayer)
        {
            SetupWander();
        }

        //If a pathnode within an enemies visible range is the partynode, start the battle sequence
        if (grid.gridFinished && !isPlayer && !gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted)
        {
            visibleRange = Physics2D.CircleCastAll(transform.position, viewRange, Vector2.zero);
            foreach (RaycastHit2D hit in visibleRange)
            {
                if (hit.transform.gameObject.GetComponent<PathNode>())
                {
                    if (hit.transform.gameObject.GetComponent<PathNode>() == gameMaster.partyNode)
                    {
                        gameMaster.LookForParticipants(gameObject);
                        lookingForParticipants = true;
                    }
                }
            }
        }

        wanderTimer -= Time.deltaTime;
        if (wander && wanderSetup && !isMoving && !isPlayer && wanderTimer <= 0 && !gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted)
        {
            Wander();
        }

        if (vectorPath.Count > 0)
        {
            hasBeenMoving = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Animator>().SetBool("Moving", true);
            }
            targetPosition = vectorPath[0];
            moveDirection = (targetPosition - (Vector2)transform.position).normalized;

            //Flipping the sprite based on the direction they are moving
            if (spriteFacingLeft)
            {
                if (moveDirection.x >= 0) //If they are moving right
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = true;
                    }
                }
            }
            else
            {
                if (moveDirection.x < 0) //If they are moving left
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = true;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
            }

            transform.Translate(pathfinding.speed * Time.deltaTime * moveDirection);

            if (lookingForParticipants)
            {
                Vector3 finishMove = vectorPath[0];
                vectorPath.Clear();
                vectorPath.Add(finishMove);
            }

            if (Vector2.Distance((Vector2)transform.position, targetPosition) < centeringOffset)
            {
                if (isPlayer)
                {
                    gameMaster.partyNode = playerPath[0];
                    playerPath.Remove(playerPath[0]);
                }
                vectorPath.Remove(vectorPath[0]);
            } 
        }

        if (vectorPath.Count == 0 && hasBeenMoving)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Animator>().SetBool("Moving", false);
            }
            isMoving = false;
            hasBeenMoving = false;

            if (gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && attackAtEnd)
            {
                attackAtEnd = false;
                gameMaster.battleMaster.GetComponent<BattleMaster>().currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                AudioManager.instance.Play(gameMaster.battleMaster.GetComponent<BattleMaster>().currentCharacter.GetComponent<CharacterSheet>().attackSound);
            }
            else if (gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && !gameMaster.battleMaster.GetComponent<BattleMaster>().currentCharacter.GetComponent<CharacterSheet>().isPlayer)
            {
                gameMaster.battleMaster.GetComponent<BattleMaster>().NextTurn();
            }
        }

        if (isPlayer && !isMoving)
        {
            if (gameMaster.targetNode != null)
            {
                MoveOnPath(pathfinding.FindPath(gameMaster.targetNode, startingNode));
                gameMaster.targetNode = null;
            }
        }
    }

    public void SetupWander()
    {
        wanderNodes = new List<PathNode>();
        raycast = Physics2D.CircleCastAll(new Vector2(transform.position.x, transform.position.y), wanderRange, Vector2.zero);
        for (int i = 0; i < raycast.Length; i++)
        {
            if (raycast[i].collider.GetComponent<PathNode>() == true)
            {
                wanderNodes.Add(raycast[i].collider.GetComponent<PathNode>());
            }
        }

        wanderSetup = true;
        Wander();
    }

    public void Wander() //Randomly pick areas within range to move to 
    {
        wanderTimer = Random.Range(wanderDelayMin, wanderDelayMax);
        wanderNode = wanderNodes[Random.Range(0, wanderNodes.Count)]; 
        if (wanderNode.occupied) //If choosing an occupied node in the wander radius, rechoose
        {
            Wander();
            return;
        }
        MoveOnPath(pathfinding.FindPath(wanderNode, startingNode));
    }

    public void MoveOnPath(List<PathNode> path)
    {
        if (path == null)
        {
            return;
        }

        isMoving = true;
        startingNode = path[path.Count- 1]; //Sets the new starting location to whereever the end position is
        if (isPlayer)
        {
            playerPath = path;
        }
        vectorPath = new List<Vector2>();

        if (gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && !gameMaster.battleMaster.GetComponent<BattleMaster>().currentCharacter.GetComponent<CharacterSheet>().isPlayer) //If a battle is happening and its an enemy's turn
        {
            if (path[0] == gameMaster.partyNode) //If the end of their path was the player, remove that node from the path and set to attack when they get one node away
            {
                attackAtEnd = true;
                path.RemoveAt(0);
            }
        }
        
        foreach (PathNode node in path)
        {
            vectorPath.Add(node.GetWorldSpace()); //Makes a vector2 list storing worldspace locations of each node on the path we want to take
        }
    }

    private void OnDrawGizmos()
    {
        //Wander range red
        grid = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>().grid;
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(startPosition, wanderRange);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, wanderRange);
        }

        //Viewing range blue
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        if (lookingForParticipants)
        {
            Gizmos.color = Color.green; //Sets the color of the distance it looks for participants
            Gizmos.DrawWireCube(transform.position, new Vector3(gameMaster.distanceToLookForParticipants, gameMaster.distanceToLookForParticipants, 0)); //Display for how far distance to look for participants is
        }
    }
}
