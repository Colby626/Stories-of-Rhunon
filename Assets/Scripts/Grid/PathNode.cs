using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode[] adjacentNodes;
    public PathNode cameFromNode;
    public List<PathNode> neighborsList = new();
    public Color baseColor;
    public bool enemyVisibleTile = false;

    private Color badColor;
    private CustomGrid grid;

    public GameMaster gameMaster;

    private void Awake()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        grid = gameMaster.grid;
        badColor = grid.GetComponent<CustomGrid>().redTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
        baseColor = GetComponentInChildren<SpriteRenderer>().color;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = badColor;
        transform.GetChild(2).GetComponent<SpriteRenderer>().color = badColor;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        transform.GetChild(1).GetComponent<SpriteRenderer>().color = baseColor;
        transform.GetChild(2).GetComponent<SpriteRenderer>().color = baseColor;
    }

    private void OnMouseDown()
    {
        if (!gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && Time.timeScale > 0) //If a battle isn't happening and the game isn't paused
        {
            gameMaster.targetNode = this;
        }
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void OnMouseOver()
    {
        if (Time.timeScale != 0) //If the game isn't paused
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public Vector2 GetWorldSpace()
    {
        return (Vector2)transform.position;
    }

    public void CreateNeighboringNodesList()
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
