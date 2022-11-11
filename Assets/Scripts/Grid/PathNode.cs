using UnityEditor.Experimental.GraphView;
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

    public GameMaster gameMaster;
    private bool isHighlighted = false;
    private void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
    }

    private void OnMouseDown()
    {
        gameMaster.GetComponent<Pathfinding>().FindPath(this);
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void OnMouseOver()
    {
        if (!isHighlighted)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            isHighlighted = true;
        }
    }

    private void OnMouseExit()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        isHighlighted = false;
    }

    public Vector2 GetWorldSpace()
    {
        return (Vector2)transform.position;
    }
}
