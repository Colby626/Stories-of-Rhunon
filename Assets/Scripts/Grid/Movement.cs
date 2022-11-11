using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Movement : MonoBehaviour //Base Movement class that certain enemy AIs can derive from
{
    public bool isPlayer;
    [Tooltip("How close to the center of a tile an entity must be to consider itself in that tile for movement")]
    public float centeringOffset = 0.1f;
    [Range(0f, 1f)]
    [Tooltip("Sprite pivots are often at the bottom instead of the middle, this is the offset upwards from the bottom of a tile")]
    public float spriteOffset = .5f;
    [Tooltip("Sets this entity to the x position relative the the custom grid")]
    public int xPosition;
    [Tooltip("Sets this entity to the y position relative the the custom grid")]
    public int yPosition;

    [Header("Enemy Stuff:")]
    public int viewRange;
    public int wanderRange;
    public bool wander = false;
    public float wanderDelay;

    private GameMaster gameMaster;
    private Pathfinding pathfinding;
    private CustomGrid grid;
    private PathNode startingNode;
    private RaycastHit2D[] raycast;
    private Collider2D[] colliders;
    private List<Vector2> vectorPath;
    private Vector2 targetPosition;
    private Vector2 moveDirection;
    private bool isMoving = false;
    private bool hasBeenMoving = false;
    private bool startPositionDetermined = false;

    private List<PathNode> wanderNodes;
    private PathNode wanderNode;
    private bool wanderSetup = false;
    private float wanderTimer;
    

    private void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        grid = gameMaster.GetComponent<GameMaster>().grid;
        pathfinding = gameMaster.GetComponent<Pathfinding>();
        //Set the transform to a grid space
        transform.position = new Vector2(grid.origin.x + xPosition + 0.5f, grid.origin.y + yPosition + spriteOffset); //Due to most pivots being at the bottom, this needs to be offset
        vectorPath = new List<Vector2>(); //For no errors in the console
    }

    private void DetermineStartPosition()
    {
        //Set the startingNode from whereever they are 
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
            DetermineStartPosition();
        }
        if (grid.gridFinished && !wanderSetup && wander && !isPlayer)
        {
            wanderSetup = true;
            SetupWander();
        }

        wanderTimer -= Time.deltaTime;
        if (wander && wanderSetup && !isMoving && !isPlayer && wanderTimer <= 0)
        {
            Wander();
        }

        if (vectorPath.Count > 0)
        {
            hasBeenMoving = true;
            targetPosition = vectorPath[0];
            moveDirection = (targetPosition - (Vector2)transform.position).normalized;
            transform.Translate(moveDirection * pathfinding.speed * Time.deltaTime);

            if (Vector2.Distance((Vector2)transform.position, targetPosition) < centeringOffset)
            {
                vectorPath.Remove(vectorPath[0]);
            }
        }

        if (vectorPath.Count == 0 && hasBeenMoving)
        {
            isMoving = false;
            hasBeenMoving = false;
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
        raycast = Physics2D.CircleCastAll(new Vector2(grid.origin.x + xPosition + 0.5f, grid.origin.y + yPosition + 0.5f), wanderRange, Vector2.zero);
        for (int i = 0; i < raycast.Length; i++)
        {
            if (raycast[i].collider.GetComponent<PathNode>() == true)
            {
                wanderNodes.Add(raycast[i].collider.GetComponent<PathNode>());
            }
        }

        Wander();
    }

    public void Wander() //Randomly pick areas within range to move to 
    {
        wanderTimer = wanderDelay;
        wanderNode = wanderNodes[Random.Range(0, wanderNodes.Count)]; 
        MoveOnPath(pathfinding.FindPath(wanderNode, startingNode));
    }

    private void MoveOnPath(List<PathNode> path)
    {
        if (path == null)
        {
            return;
        }

        isMoving = true;
        startingNode = path[path.Count- 1]; //Sets the new starting location to whereever the end position is
        if (isPlayer)
        {
            gameMaster.partyNode = startingNode; 
        }
        vectorPath = new List<Vector2>();
        
        foreach (PathNode node in path)
        {
            vectorPath.Add(node.GetWorldSpace()); //Makes a vector2 list storing worldspace locations of each node on the path we want to take
        }
    }

    public void Approach()
    {
        //Move towards the player's party
        //If player party within view range, wander = false;

        //Find the node closest to the player's party from the side the enemy is on and start moving towards it 
        //If player party leaves view range, wander = true;
    }

    private void OnDrawGizmosSelected()
    {
        grid = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>().grid;
        Gizmos.DrawWireSphere(new Vector2(grid.origin.x + xPosition + 0.5f, grid.origin.y + yPosition + 0.5f), wanderRange);
    }
}