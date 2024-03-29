using System.Collections.Generic; //For lists
using System.Linq;
using UnityEngine;

public class Movement : MonoBehaviour
{
    /* For when the pathfinding is on each character
    struct pathfindingNodes
    {
        int gCost;
        int hCost;
        int fCost;
    };

    private pathfindingNodes[][] pathfindingNodeArray;
    /*   0   0   0   0   0   0      each 0 has a g, h, and f 
     *   0   0   0   0   0   0
     *   0   0   0   0   0   0
     *   0   0   0   0   0   0
     *   0   0   0   0   0   0
     *   0   0   0   0   0   0
     */

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

    private bool hasBeenMoving = false;
    private bool wanderSetup = false;
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool attackAtEnd = false;
    [HideInInspector]
    public bool endTurnAfterMove = false;
    [HideInInspector]
    public PathNode startingNode;
    [HideInInspector]
    public List<Vector2> vectorPath;
    [HideInInspector]
    public bool openChestAtEnd = false;

    private Vector3 startPosition;
    private Vector2 targetPosition;
    private Vector2 moveDirection;

    private List<PathNode> wanderNodes;
    private PathNode wanderNode;
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
        grid.gridFinishedEvent.AddListener(DetermineStartPosition);
    }

    private void Update()
    {
        if (grid.gridFinished)
        {
            SetupMovement(); 
        }

        wanderTimer -= Time.deltaTime;
        if (wander && wanderSetup && !isMoving && !isParty && wanderTimer <= 0 && !battleMaster.battleStarted)
        {
            Wander();
        }

        Translation(); //Handles the actual movement of the gameObject in the scene
        PostTranslation(); //Handles stopping

        if (isParty && !isMoving) //Party movement
        {
            if (gameMaster.targetNode != null)
            {
                MoveOnPath(pathfinding.FindPath(gameMaster.targetNode, startingNode, pathfinding.furthestAnyoneCanMove));
                gameMaster.targetNode = null;
            }
        }
    }

    private void SetupMovement()
    {
        if (!isParty)
        {            
            if (!lookingForParticipants)
            {
                PlayerInRangeCheck();
            }
        }
        OccupyingNodeCheck();
    }

    private void DetermineStartPosition()
    {
        //Set the startingNode from wherever they are 
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.1f, 0.1f), 0);
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

        if (!isParty && wander)
        {
            SetupWander();
        }
    }

    public void MoveOnPath(List<PathNode> path)
    {
        if (path == null)
        {
            Debug.LogWarning("Path was null");
            return;
        }

        isMoving = true;
        startingNode = path[path.Count - 1]; //Sets the new starting location to whereever the end position is
        vectorPath = new List<Vector2>();

        foreach (PathNode node in path)
        {
            vectorPath.Add(node.GetWorldSpace()); //Makes a vector2 list storing worldspace locations of each node on the path we want to take
        }
    }

    private void OccupyingNodeCheck() //Biggest performance hit during general play 
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
        occupyingNode.destinationNode = false;
        occupyingNode.occupyingAgent = gameObject;
    }

    private void PlayerInRangeCheck()
    {
        if (!lookingForParticipants)
        {
            //If a pathnode within an enemies visible range is the partynode, start the battle sequence
            //If you remove !battleMaster.battleStarted, it is a full crash
            if (!battleMaster.battleStarted) //Could change the repeated raycast into a large collider and use OnTriggerEnter to do this
            {
                RaycastHit2D[] visibleRange = Physics2D.CircleCastAll(transform.position, viewRange, Vector2.zero);
                foreach (RaycastHit2D hit in visibleRange)
                {
                    if (hit.transform.gameObject.GetComponent<PathNode>())
                    {
                        if (hit.transform.gameObject.GetComponent<PathNode>() == gameMaster.partyNode)
                        {
                            if (wanderNode != null)
                            {
                                wanderNode.destinationNode = false;
                            }
                            lookingForParticipants = true;
                            gameMaster.LookForParticipants(gameObject); 
                        }
                    }
                }
            }
            else if (!gameMaster.participants.Contains(gameObject) && battleMaster.currentCharacter.isPlayer) //Joining battles in progress
            {
                RaycastHit2D[] visibleRange = Physics2D.CircleCastAll(transform.position, viewRange, Vector2.zero);
                foreach (RaycastHit2D hit in visibleRange)
                {
                    if (hit.transform.gameObject.GetComponent<PathNode>())
                    {
                        if (hit.transform.gameObject.GetComponent<PathNode>() == gameMaster.partyNode)
                        {
                            lookingForParticipants = true;
                            gameMaster.JoinBattle(gameObject);
                        }
                    }
                }
            }
        }
    }

    private void Translation()
    {
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

            if (Vector2.Distance((Vector2)transform.position, targetPosition) < centeringOffset)
            {
                if (isParty)
                {
                    gameMaster.partyNode = occupyingNode;
                }
                vectorPath.Remove(vectorPath[0]);
            }
        }
    }

    private void PostTranslation()
    {
        if (vectorPath.Count == 0 && hasBeenMoving)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Animator>().SetBool("Moving", false);
            }
            //Snap to center of node
            transform.position = occupyingNode.transform.position;

            hasBeenMoving = false;

            if (battleMaster.battleStarted && !gameMaster.waitingOnEveryoneToStop)
            {
                if (attackAtEnd)
                {
                    attackAtEnd = false;
                    CharacterSheet currentCharacter = battleMaster.currentCharacter;

                    if (currentCharacter.isPlayer)
                    {
                        //If the player is to the right of the enemy
                        if (currentCharacter.transform.position.x - battleMaster.targetedEnemy.transform.position.x < 0)
                        {
                            //If the enemy is facing left flip them
                            if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                            }
                            else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                            }
                        }
                        //If the player is to the left or directly above/below the enemy
                        if (battleMaster.currentCharacter.transform.position.x - battleMaster.targetedEnemy.transform.position.x >= 0)
                        {
                            //If the enemy is facing right flip them
                            if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                            }
                            else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                            }
                        }
                    }
                    else
                    {
                        //If the player is to the right of the enemy
                        if (currentCharacter.transform.position.x - battleMaster.targetedPlayer.transform.position.x < 0)
                        {
                            //If the enemy is facing left flip them
                            if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                            }
                            else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                            }
                        }
                        //If the player is to the left or directly above/below the enemy
                        if (battleMaster.currentCharacter.transform.position.x - battleMaster.targetedPlayer.transform.position.x >= 0)
                        {
                            //If the enemy is facing right flip them
                            if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == false && currentCharacter.GetComponent<SpriteRenderer>().flipX == false)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = true;
                            }
                            else if (currentCharacter.GetComponentInParent<Movement>().spriteFacingLeft == true && currentCharacter.GetComponent<SpriteRenderer>().flipX == true)
                            {
                                currentCharacter.GetComponent<SpriteRenderer>().flipX = false;
                            }
                        }
                    }

                    currentCharacter.GetComponent<Animator>().SetTrigger("StartAttack");
                    battleMaster.attackDone = true;
                    AudioManager.instance.Play(battleMaster.currentCharacter.attackSound);
                }
                else if (!battleMaster.currentCharacter.isPlayer && endTurnAfterMove)
                {
                    endTurnAfterMove = false;
                    battleMaster.NextTurn();
                }
            }
            if (openChestAtEnd)
            {
                battleMaster.OpenChestMenu();
                openChestAtEnd = false;
            }
            isMoving = false;
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
        if (numberOfOccupiedNeighbors != occupyingNode.GetNeighborNodes().Count) //If not surrounded
        {
            wanderTimer = Random.Range(wanderDelayMin, wanderDelayMax);
            wanderNode = wanderNodes[Random.Range(0, wanderNodes.Count)];
            if (wanderNode.occupied || wanderNode.destinationNode || wanderNode.chest != null) //If choosing an occupied node in the wander radius or one that contains a chest, rechoose
            {
                Wander();
                return;
            }
            wanderNode.destinationNode = true;
            MoveOnPath(pathfinding.FindPath(wanderNode, startingNode, pathfinding.furthestAnyoneCanMove));
        }
        else //If surrounded, wait and try again
        {
            wanderTimer = Random.Range(wanderDelayMin, wanderDelayMax);
        }
    }

    #if (UNITY_EDITOR)

    private void OnDrawGizmos()
    {
        //Wander range red
        grid = FindObjectOfType<GameMaster>().GetComponent<GameMaster>().grid;
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
            //The minus 0.1f is to prevent the gizmo from going under the grid 
            Gizmos.DrawWireCube(transform.position, new Vector3(distanceToLookForParticipants-0.1f, distanceToLookForParticipants-0.1f, 0)); //Display for how far distance to look for participants is
        }

        /* furthestAnyoneCanMove and therefore the distance away from an enemy before they give up
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 20);
        */
    }

   #endif
}