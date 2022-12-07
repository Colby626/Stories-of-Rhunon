using System.Collections.Generic; //For lists
using System.Linq;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private bool isParty;
    [Tooltip("How close to the center of a tile an entity must be to consider itself in that tile for movement.")]
    [SerializeField]
    private float centeringOffset = 0.1f;
    public bool spriteFacingLeft;

    public PathNode occupyingNode;
    [Header("Enemy Stuff:")]
    [Tooltip("Blue circle")]
    [SerializeField]
    private int viewRange;
    [Tooltip("Red circle")]
    [SerializeField]
    private int wanderRange;
    [SerializeField]
    private bool wander = false;
    [SerializeField]
    private float wanderDelayMin;
    [SerializeField]
    private float wanderDelayMax;
    [Tooltip("Green box (will appear if lookingForParticipants is true")]
    public int distanceToLookForParticipants = 15;
    public bool lookingForParticipants = false;

    private bool isMoving = false;
    private bool hasBeenMoving = false;
    private bool startPositionDetermined = false;
    private bool wanderSetup = false;
    [HideInInspector]
    public bool attackAtEnd = false;

    private PathNode startingNode;
    private Vector3 startPosition;
    private Vector2 targetPosition;
    private Vector2 moveDirection;
    private List<PathNode> playerPath;
    [HideInInspector]
    public List<Vector2> vectorPath;
    
    private List<PathNode> wanderNodes;
    private float wanderTimer;

    private GameMaster gameMaster;
    private BattleMaster battleMaster;
    private Pathfinding pathfinding;
    private CustomGrid grid;

    private void Start()
    {
        startPosition = transform.position;
        gameMaster = GameMaster.instance.GetComponent<GameMaster>();
        battleMaster = FindObjectOfType<BattleMaster>().GetComponent<BattleMaster>();
        grid = FindObjectOfType<CustomGrid>().GetComponent<CustomGrid>();
        pathfinding = FindObjectOfType<Pathfinding>().GetComponent<Pathfinding>();
        vectorPath = new List<Vector2>(); //Prevents an error in the console
    }

    private void DetermineStartPosition()
    {
        //Set the startingNode from wherever they are 
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.5f, 0.5f), 0);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetComponent<PathNode>() == true)
            {
                startingNode = colliders[i].GetComponent<PathNode>();
                occupyingNode = startingNode;
                break;
            }
        }

        if (startingNode == null)
        {
            Debug.LogError("Starting position not determined. Character not over the grid.");
        }

        if (isParty)
        {
            gameMaster.partyNode = startingNode;
        }
    }

    private void Update()
    {
        if (grid.gridFinished && !startPositionDetermined) //Make gridFinished a UnityEvent
        {
            startPositionDetermined = true;
            gameMaster.startPositionDetermined = true;
            DetermineStartPosition();
        }
        if (grid.gridFinished) //Surely this can be more performant than raycasting in update to find the occupyingNode, but putting it in the occupying node OnTriggerEnter2D wasn't consistant
        {
            int closestNode = 0;
            float minDistance = float.PositiveInfinity;
            RaycastHit2D[] findNode = Physics2D.CircleCastAll(transform.position, 1.5f, Vector2.zero);

            // Setting occupied node to the nearest center of a node
            for (int i = 0; i < findNode.Count(); i++)
            {
                if (findNode[i].transform.GetComponent<PathNode>() is PathNode pathNode)
                {
                    if (pathNode.occupyingAgent == null || pathNode.occupyingAgent == gameObject)
                    {
                        pathNode.occupied = false;
                        pathNode.occupyingAgent = null;
                        pathNode.transform.GetChild(1).gameObject.SetActive(true);
                        pathNode.transform.GetChild(2).gameObject.SetActive(true);
                    }
                    if (Vector3.Distance(transform.position, pathNode.transform.position) < minDistance)
                    {
                        minDistance = Vector3.Distance(transform.position, pathNode.transform.position);
                        closestNode = i;
                    }
                }
            }

            occupyingNode = findNode[closestNode].transform.GetComponent<PathNode>();
            occupyingNode.occupied = true;
            occupyingNode.destinationNode= false;
            occupyingNode.occupyingAgent = gameObject;
            occupyingNode.transform.GetChild(1).gameObject.SetActive(false);
            occupyingNode.transform.GetChild(2).gameObject.SetActive(false);
        }
        if (grid.gridFinished && !wanderSetup && wander && !isParty)
        {
            SetupWander();
        }

        //If a pathnode within an enemies visible range is the partynode, start the battle sequence
        if (grid.gridFinished && !isParty && !battleMaster.battleStarted && !lookingForParticipants) //Could change the repeated raycast into a large collider and use OnTriggerEnter to do this
        {
            RaycastHit2D[] visibleRange = Physics2D.CircleCastAll(transform.position, viewRange, Vector2.zero);
            foreach (RaycastHit2D hit in visibleRange)
            {
                if (hit.transform.gameObject.GetComponent<PathNode>())
                {
                    if (hit.transform.gameObject.GetComponent<PathNode>() == gameMaster.partyNode) 
                    {
                        lookingForParticipants = true;
                        gameMaster.LookForParticipants(gameObject);
                    }
                }
            }
        }

        wanderTimer -= Time.deltaTime;
        if (wander && wanderSetup && !isMoving && !isParty && wanderTimer <= 0 && !battleMaster.battleStarted)
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
                if (isParty)
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
            //Snap to center of node
            transform.position = occupyingNode.transform.position;

            isMoving = false;
            hasBeenMoving = false;

            if (battleMaster.battleStarted && attackAtEnd)
            {
                attackAtEnd = false;
                battleMaster.currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                AudioManager.instance.Play(battleMaster.currentCharacter.attackSound);
            }
            else if (battleMaster.battleStarted && !battleMaster.currentCharacter.isPlayer)
            {
                battleMaster.NextTurn();
            }
        }

        if (isParty && !isMoving)
        {
            if (gameMaster.targetNode != null)
            {
                MoveOnPath(pathfinding.FindPath(gameMaster.targetNode, startingNode));
                gameMaster.targetNode = null;
            }
        }
    }

    private void SetupWander()
    {
        wanderNodes = new List<PathNode>();
        RaycastHit2D[] raycast = Physics2D.CircleCastAll(new Vector2(transform.position.x, transform.position.y), wanderRange, Vector2.zero);
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

    private void Wander() //Randomly pick areas within range to move to 
    {
        int numberOfOccupiedNeighbors = 0;
        foreach (PathNode neighborNode in occupyingNode.GetNeighborNodes())
        {
            if (neighborNode.occupied == false)
            {
                break;
            }
            else
            {
                numberOfOccupiedNeighbors += 1;
            }
        }
        if (numberOfOccupiedNeighbors != occupyingNode.GetNeighborNodes().Count)
        {
        
            wanderTimer = Random.Range(wanderDelayMin, wanderDelayMax);
            PathNode wanderNode = wanderNodes[Random.Range(0, wanderNodes.Count)];
            if (wanderNode.occupied || wanderNode.destinationNode) //If choosing an occupied node in the wander radius, rechoose
            {
                Wander();
                return;
            }
            wanderNode.destinationNode = true;
            MoveOnPath(pathfinding.FindPath(wanderNode, startingNode));
        }
    }

    public void MoveOnPath(List<PathNode> path)
    {
        if (path == null)
        {
            return;
        }

        isMoving = true;
        startingNode = path[path.Count- 1]; //Sets the new starting location to whereever the end position is
        if (isParty)
        {
            playerPath = path;
        }
        vectorPath = new List<Vector2>();

        if (battleMaster.battleStarted)
        {
            if (!battleMaster.currentCharacter.isPlayer) //If a battle is happening and its an enemy's turn)
            {
                if (path[0] == gameMaster.partyNode) //If the end of their path was the player, remove that node from the path and set to attack when they get one node away
                {
                    attackAtEnd = true;
                    path.RemoveAt(0);
                }
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
            Gizmos.DrawWireCube(transform.position, new Vector3(distanceToLookForParticipants, distanceToLookForParticipants, 0)); //Display for how far distance to look for participants is
        }
    }
}
