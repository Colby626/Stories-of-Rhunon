using UnityEngine;
using UnityEngine.UIElements;

public class CustomGrid : MonoBehaviour
{
    public int numColumns;
    public int numRows;
    [HideInInspector]
    private const int cellSize = 1;
    public Vector3 origin;
    public GameObject goodTile;
    public GameObject badTile;

    private Collider2D[] colliders;
    private bool foundThing = false;
    private void Start()
    {
        for (int x = 0; x < numColumns; x++)
        {
            for (int y = 0; y < numRows; y++)
            {
                //If where we are instantiating a new tile is nothing check the next spot in the list
                if (Physics2D.OverlapBox(new Vector2(x + origin.x + .5f, y + origin.y + .5f), new Vector2(0.5f, 0.5f), 0) == null)
                {
                    continue;
                }

                //Puts all colliders within a tile in a colliders array
                colliders = Physics2D.OverlapBoxAll(new Vector2(x + origin.x + .5f, y + origin.y + .5f), new Vector2(0.5f, 0.5f), 0);

                foundThing = false;
                //For the length of the collider array
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].CompareTag("UnWalkable"))
                    {
                        GameObject instance = Instantiate(badTile, new Vector2(x + origin.x + .5f, y + origin.y + .5f), Quaternion.identity);
                        instance.transform.localScale = new Vector3(.5f, .5f, 0);
                        instance.AddComponent<BoxCollider2D>().size *= 2;
                        foundThing = true;
                    }
                }

                //If there are no unwalkable colliders within a tile
                if (foundThing == false)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].CompareTag("Walkable"))
                        {
                            GameObject instance = Instantiate(goodTile, new Vector2(x + origin.x + .5f, y + origin.y + .5f), Quaternion.identity);
                            instance.transform.localScale = new Vector3(.5f, .5f, 0);
                            instance.AddComponent<BoxCollider2D>().size *= 2;
                            foundThing = true;
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Draw the grid in the editor with Gizmos.DrawLines
        for (int x = 0; x < numColumns*cellSize; x+=cellSize)
        {
            for (int y = 0; y < numRows*cellSize; y+=cellSize)
            {
                Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y), new Vector2(x + origin.x, y + origin.y + cellSize));
                Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y), new Vector2(x + origin.x + cellSize, y + origin.y));

                if(y == numRows*cellSize - cellSize)
                {
                    Gizmos.DrawLine(new Vector2(x + origin.x, y + origin.y + cellSize), new Vector2(x + origin.x + cellSize, y + origin.y + cellSize));
                }
                if (x == numColumns*cellSize - cellSize)
                {
                    Gizmos.DrawLine(new Vector2(x + origin.x + cellSize, y + origin.y), new Vector2(x + origin.x + cellSize, y + origin.y + cellSize));
                }
            }
        }
    }
}
