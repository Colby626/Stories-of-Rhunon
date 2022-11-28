using UnityEditor;
using UnityEngine;

public class CustomGrid : MonoBehaviour
{
    public int numColumns;
    public int numRows;
    public Vector2Int origin;
    public GameObject whiteTile;
    public GameObject redTile;
    public GameObject blueTile;
    [HideInInspector]
    public bool gridFinished = false;
    public bool gridClicked = false;

    private const int cellSize = 1;
    private PathNode[,] nodes;
    private Collider2D[] colliders;
    private bool tilePlaced;

    private void Start()
    {
        nodes = new PathNode[numColumns, numRows];
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                //If where we are instantiating a new tile is nothing check the next spot in the list
                if (Physics2D.OverlapBox(new Vector2(x + origin.x + .5f, y + origin.y + .5f), new Vector2(0.001f, 0.001f), 0) == null)
                {
                    continue;
                }

                //Puts all colliders within a tile in a colliders array
                tilePlaced = false;
                colliders = Physics2D.OverlapBoxAll(new Vector2(x + origin.x + .5f, y + origin.y + .5f), new Vector2(0.5f, 0.5f), 0);

                //Don't display anything on tiles that contain something with unwalkable on it
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].CompareTag("UnWalkable"))
                    {
                        tilePlaced = true;
                        break;
                    }
                }

                //If there are no unwalkable colliders within a tile
                if (!tilePlaced)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].CompareTag("Walkable"))
                        {
                            GameObject instance = Instantiate(whiteTile, new Vector2(x + origin.x + .5f, y + origin.y + .5f), Quaternion.identity);
                            instance.transform.localScale = new Vector3(.5f, .5f, 0);
                            instance.GetComponent<PathNode>().x = x;
                            instance.GetComponent<PathNode>().y = y;
                            nodes[x, y] = instance.GetComponent<PathNode>();
                            instance.name = x + " " + y;
                            break; //Removing this causes Unity to crash
                        }
                    }
                }
            }
        }
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                if (nodes[x, y] != null)
                {
                    nodes[x, y].CreateNeighboringNodesList();
                }
            }
        }
        gridFinished = true;
    }

    public void SetGridClicked()
    {
        gridClicked = true;
    }

    public PathNode GetGridObject(int x, int y)
    {
        if (x < 0 || y < 0)
        {
            return null;   
        }
        return nodes[x, y];
    }

    public int GetGridWidth()
    {
        return numColumns - 1;
    }

    public int GetGridHeight()
    {
        return numRows - 1;
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x < numColumns; x+=5) //Drawing every tile cripples performance
        {
            for (int y = 0; y < numRows; y+=5) //Drawing every tile cripples performance
            {
                Handles.Label(new Vector2(origin.x + x, origin.y + y + cellSize), x + " " + y);
            }
        }
        //Draw the grid in the editor with Gizmos.DrawLines
        for (int x = 0; x < numColumns * cellSize; x += cellSize)
        {
            for (int y = 0; y < numRows * cellSize; y += cellSize)
            {
                //Drawing the boxes
                Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y), new Vector2(x + origin.x, y + origin.y + cellSize));
                Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y), new Vector2(x + origin.x + cellSize, y + origin.y));

                if (y == numRows * cellSize - cellSize)
                {
                    Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y + cellSize), new Vector2(x + origin.x + cellSize, y + origin.y + cellSize));
                }
                if (x == numColumns * cellSize - cellSize)
                {
                    Gizmos.DrawLine(new Vector2(x + origin.x + cellSize, y + origin.y), new Vector2(x + origin.x + cellSize, y + origin.y + cellSize));
                }
            }
        }
    }
}
