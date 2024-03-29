using System.Collections.Generic; //For lists
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private const int DIAGONAL_MOVEMENT_COST = 14;
    private const int STRAIGHT_MOVEMENT_COST = 10;

    private List<PathNode> openList;
    private HashSet<PathNode> closedList;
    [HideInInspector]
    public List<PathNode> path;

    [HideInInspector]
    public Collider2D[] colliders;
    public float speed = .01f;
    [Tooltip("The smaller this number the better the performance")]
    public int furthestAnyoneCanMove = 20; //99 speed
    [HideInInspector]
    public PathNode lastNodeRemoved;

    [HideInInspector]
    public GameMaster gameMaster;
    [HideInInspector]
    public CustomGrid grid;

    public List<PathNode> FindPath(PathNode endNode, PathNode startNode, int maxNodes, bool ignoreOccupied = false)
    {
        /* I've never seen this error message, so I will comment it out for performance
        if (endNode == null)
        {
            Debug.LogWarning("endNode was null");
            return null;
        }
        */
        if (startNode == endNode)
        {
            return new List<PathNode> { endNode };
        }
        
        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode> { };

        //I don't know why *2 is needed, but otherwise it does half
        for (int x = startNode.x - furthestAnyoneCanMove*2; x < startNode.x + furthestAnyoneCanMove*2; x++)
        {
            for (int y = startNode.y - furthestAnyoneCanMove*2; y < startNode.y + furthestAnyoneCanMove*2; y++)
            {
                PathNode node = grid.GetGridObject(x, y);
                if (node != null)
                {
                    node.gCost = int.MaxValue;
                    node.CalculateFCost();
                    node.cameFromNode = null;
                }
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
                if (endNode.occupied) 
                {
                    return TrimPath(CalculatePath(endNode), maxNodes);
                }
                if (endNode.chest != null) //Only when a player clicks on a chest outside of battle
                {
                    List<PathNode> finalPath = CalculatePath(endNode);
                    finalPath.RemoveAt(finalPath.Count - 1);
                    return TrimPath(finalPath, furthestAnyoneCanMove);
                }
                return TrimPath(CalculatePath(endNode), maxNodes);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in currentNode.GetNeighborNodes())
            {
                if (ignoreOccupied == false && neighborNode.occupied && neighborNode != gameMaster.partyNode || neighborNode.chest != null && neighborNode != endNode)
                {
                    closedList.Add(neighborNode);
                }
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
        return null;
    }

    private List<PathNode> CalculatePath(PathNode endNode)
    {
        List<PathNode> path = new List<PathNode> { endNode };
        PathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private List<PathNode> TrimPath(List<PathNode> path, int maxNodes)
    {
        lastNodeRemoved = null;
        while (path.Count > maxNodes)
        {
            lastNodeRemoved = path[path.Count - 1];
            path.RemoveAt(path.Count - 1);
        }

        if (path[path.Count - 1].occupied)
        {
            while (path[path.Count - 1].occupied) //For when ignoreOccupied == true 
            {
                if (path.Count == 1) //Prevents an error when right next to the target
                {
                    break;
                }
                lastNodeRemoved = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);
            }
        }
        return path;
    }

    public int CalculateDistance(PathNode a, PathNode b)
    {
        /* I've never seen this error message so I'll comment it to be more efficient
        if (a == null || b == null)
        {
            Debug.LogWarning("CalculateDistance called on null node");
            return -1;
        }
        */

        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return DIAGONAL_MOVEMENT_COST * Mathf.Min(xDistance, yDistance) + STRAIGHT_MOVEMENT_COST * remaining;
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
}