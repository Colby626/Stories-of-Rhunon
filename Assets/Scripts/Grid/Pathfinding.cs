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
    private CustomGrid grid;
    public float speed = .01f;

    public List<PathNode> FindPath(PathNode endNode, PathNode startNode) //Will need to make sure that locations gameobjects tagged as participants are on are not put into a path to traverse
    {
        if (endNode == null)
        {
            return null;
        }
        if (startNode == endNode)
        {
            return CalculatePath(endNode);
        }
        grid = GetComponent<GameMaster>().grid;
        if (endNode.occupied)
        {
            Vector2 temp = new Vector2(startNode.x, startNode.y) - new Vector2(endNode.x, endNode.y);
            int xSign = 0;
            int ySign = 0;
            if (temp.x < 0)
            {
                xSign--;
            } 
            else if (temp.x > 0)
            {
                xSign++;
            }
            if (temp.y < 0)
            {
                ySign--;
            }
            else if (temp.y > 0)
            {
                ySign++;
            }
            
            return FindPath(grid.GetGridObject(xSign + endNode.x, ySign + endNode.y), startNode);
        }
        
        openList = new List<PathNode> { startNode };
        closedList = new HashSet<PathNode> { };

        for (int x = 0; x < GetComponent<GameMaster>().grid.numColumns; x++)
        {
            for (int y = 0; y < GetComponent<GameMaster>().grid.numRows; y++)
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
        int i = 0;

        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighborNode in currentNode.GetNeighborNodes())
            {
                if (neighborNode.occupied)
                {
                    closedList.Add(neighborNode);
                }
                if (closedList.Contains(neighborNode))
                {
                    continue;
                }

                i++;

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

    private int CalculateDistance(PathNode a, PathNode b)
    {
        if (a == null || b == null)
        {
            Debug.LogWarning("CalculateDistance called on null node");
            return -1;
        }

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
