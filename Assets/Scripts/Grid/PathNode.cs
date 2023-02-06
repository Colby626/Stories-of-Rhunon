using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool occupied = false;
    public GameObject occupyingAgent;
    public bool destinationNode = false;
    public bool validMovePosition;

    public PathNode cameFromNode;
    public List<PathNode> neighborsList = new();

    private CustomGrid grid;

    private void Awake()
    {
        grid = FindObjectOfType<CustomGrid>().GetComponent<CustomGrid>();
        validMovePosition = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public Vector2 GetWorldSpace()
    {
        return (Vector2)transform.position;
    }

    public void CreateNeighboringNodesList() //do pathfinding grid once, have a list of participants
    {
        //Check Left
        if (x - 1 >= 0)
        {
            if (grid.GetGridObject(x - 1, y) != null)
            {
                neighborsList.Add(grid.GetGridObject(x - 1, y));
            }

            //Check Left Down
            if (y - 1 >= 0 && grid.GetGridObject(x - 1, y - 1) != null)
            {
                neighborsList.Add(grid.GetGridObject(x - 1, y - 1));
            }

            //Check Left Up
            if (y + 1 <= grid.GetGridHeight() && grid.GetGridObject(x - 1, y + 1) != null)
            {
                neighborsList.Add(grid.GetGridObject(x - 1, y + 1));
            }
        }

        //Check Right
        if (x + 1 <= grid.GetGridWidth())
        {
            if (grid.GetGridObject(x + 1, y) != null)
            {
                neighborsList.Add(grid.GetGridObject(x + 1, y));
            }

            //Check Right Down
            if (y - 1 >= 0 && grid.GetGridObject(x + 1, y - 1) != null)
            {
                neighborsList.Add(grid.GetGridObject(x + 1, y - 1));
            }

            //Check Right Up
            if (y + 1 <= grid.GetGridHeight() && grid.GetGridObject(x + 1, y + 1) != null)
            {
                neighborsList.Add(grid.GetGridObject(x + 1, y + 1));
            }
        }

        //Check Down
        if (y - 1 >= 0 && grid.GetGridObject(x, y - 1) != null)
        {
            neighborsList.Add(grid.GetGridObject(x, y - 1));
        }

        //Check Up
        if (y + 1 <= grid.GetGridHeight() && grid.GetGridObject(x, y + 1) != null)
        {
            neighborsList.Add(grid.GetGridObject(x, y + 1));
        }
    }

    public List<PathNode> GetNeighborNodes()
    {
        return neighborsList;
    }
}
