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
    public bool validMovePosition;
    public PathNode[] adjacentNodes;
    public PathNode cameFromNode;
    public List<PathNode> neighborsList = new();

    private CustomGrid grid;
    public GameMaster gameMaster;

    private void Awake()
    {
        gameMaster = GameMaster.instance.GetComponent<GameMaster>();
        grid = gameMaster.grid;
        validMovePosition = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void OnMouseOver()
    {
        if (!gameMaster.hoveringOverButton) //Deadzone for mouse at the bottom of the screen
        {
            if (Time.timeScale != 0 && !occupied) //If the game isn't paused
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
            if (grid.gridClicked)
            {
                if (!gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && !occupied && Time.timeScale > 0) //If a battle isn't happening and the game isn't paused
                {
                    gameMaster.targetNode = this;
                }

                if (gameMaster.battleMaster.GetComponent<BattleMaster>().battleStarted && !occupied && Time.timeScale > 0 && validMovePosition && !gameMaster.movedOnTurn && gameMaster.battleMaster.GetComponent<BattleMaster>().currentCharacter.GetComponent<CharacterSheet>().isPlayer)
                {
                    gameMaster.targetNode = this;
                    gameMaster.movedOnTurn = true;
                }
                grid.gridClicked = false;
            }
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
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
