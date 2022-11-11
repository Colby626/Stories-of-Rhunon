using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class Pathfinding : MonoBehaviour
{
    private const int diagonalMovementCost = 14;
    private const int straightMovementCost = 10;

    private List<PathNode> openList;
    private HashSet<PathNode> closedList;
    public List<PathNode> path;

    public Collider2D[] colliders;
    public PathNode partyNode;
    public GameObject party;
    private bool isPlayer = false;
    private CustomGrid grid;
    public float speed = .01f;
    private GameObject entity;
    private List<Vector2> vectorPath = new();
    private Vector2 moveDirection;
    private Vector2 targetPosition;
    private bool hasBeenMoving = false;
    private bool isMoving = false;

    public void FindPath(PathNode endNode, PathNode startNode = null)
    {
        if (isMoving)
        {
            Debug.Log("Already moving");
            return;
        }

        grid = GetComponent<GameMaster>().grid;

        //if the player is the one doing the pathfinding
        if (startNode == null)
        {
            colliders = Physics2D.OverlapBoxAll(new Vector2(grid.origin.x + GetComponent<GameMaster>().partyX + 0.5f, grid.origin.y + GetComponent<GameMaster>().partyY + 0.5f), new Vector2(0.5f, 0.5f), 0);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<PathNode>() == true)
                {
                    partyNode = colliders[i].GetComponent<PathNode>();
                }
            }

            if (partyNode == null)
            {
                Debug.LogError("No starting node for pathfinding");
                return;
            }
            isPlayer = true;
            startNode = partyNode;
        }

        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode> { };

        for (int x = 0; x < GetComponent<GameMaster>().grid.numColumns; x++)
        {
            for (int y = 0; y < GetComponent<GameMaster>().grid.numRows; y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistance(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                if (isPlayer)
                {
                    MoveOnPath(CalculatePath(endNode), party); //Currently only moving the party 
                }
                else
                {
                    //MoveOnPath(CalculatePath(endNode), enemy);
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in GetNeighboringNodes(currentNode))
            {
                if (closedList.Contains(neighborNode))
                {
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistance(currentNode, neighborNode);
                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistance(neighborNode, endNode);
                    neighborNode.CalculateFCost();
                }

                if (!openList.Contains(neighborNode))
                {
                    openList.Add(neighborNode);
                }
            }
        }

        //Out of nodes on the open list
        return;
    }

    private List<PathNode> GetNeighboringNodes(PathNode currentNode) //Can be optimized by doing this step when creating the grid to begin with 
    {
        List<PathNode> neighborsList = new List<PathNode>();

        //Check Left
        if (currentNode.x - 1 >= 0)
        {
            neighborsList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y));

            //Check Left Down
            if (currentNode.y - 1 >= 0)
            {
                neighborsList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y - 1));
            }

            //Check Left Up
            if (currentNode.y + 1 <= grid.GetGridHeight())
            {
                neighborsList.Add(grid.GetGridObject(currentNode.x - 1, currentNode.y + 1));
            }
        }

        //Check Right
        if (currentNode.x + 1 <= grid.GetGridWidth())
        {
            neighborsList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y));

            //Check Right Down
            if (currentNode.y - 1 >= 0)
            {
                neighborsList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y - 1));
            }

            //Check Right Up
            if (currentNode.y + 1 <= grid.GetGridHeight())
            {
                neighborsList.Add(grid.GetGridObject(currentNode.x + 1, currentNode.y + 1));
            }
        }

        //Check Down
        if (currentNode.y - 1 >= 0)
        {
            neighborsList.Add(grid.GetGridObject(currentNode.x, currentNode.y - 1));
        }

        //Check Up
        if (currentNode.y + 1 <= grid.GetGridHeight())
        {
            neighborsList.Add(grid.GetGridObject(currentNode.x, currentNode.y + 1));
        }

        return neighborsList;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistance(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return diagonalMovementCost * Mathf.Min(xDistance, yDistance) + straightMovementCost * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }

        return lowestFCostNode;
    }

    private void MoveOnPath(List<PathNode> path, GameObject thingToMove)
    {
        isMoving = true;
        entity = thingToMove;
        vectorPath = new List<Vector2>();
        if (path == null)
        {
            return;
        }

        foreach (PathNode node in path)
        {
            vectorPath.Add(node.GetWorldSpace()); //Makes a vector2 list storing worldspace locations of each node on the path we want to take
        }

        if (isPlayer)
        {
            partyNode = path[path.Count - 1];
        }
    }

    private void Update()
    {
        if (vectorPath.Count > 0)
        {
            hasBeenMoving = true;
            targetPosition = vectorPath[0];
            moveDirection = (targetPosition - (Vector2)entity.transform.position).normalized;
            entity.transform.Translate(moveDirection * speed * Time.deltaTime);

            if (Vector2.Distance((Vector2)entity.transform.position, targetPosition) < 0.01f)
            {
                vectorPath.Remove(vectorPath[0]);
            }
        }

        if (vectorPath.Count == 0 && hasBeenMoving)
        {
            isMoving = false;
            GetComponent<GameMaster>().partyX = partyNode.x;
            GetComponent<GameMaster>().partyY = partyNode.y;
            hasBeenMoving = false;
        }
    }
}
