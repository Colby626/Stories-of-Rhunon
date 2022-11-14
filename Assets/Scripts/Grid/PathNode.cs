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

    private Color baseColor;
    private Color badColor;
    private CustomGrid grid;

    public GameMaster gameMaster;

    private void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GameMaster").GetComponent<GameMaster>();
        grid = gameMaster.grid;
        badColor = grid.GetComponent<CustomGrid>().badTile.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
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
        gameMaster.targetNode = this;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void OnMouseOver()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public Vector2 GetWorldSpace()
    {
        return (Vector2)transform.position;
    }
}
