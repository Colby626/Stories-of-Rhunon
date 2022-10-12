using System.Collections.Generic; //For lists
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new();

    // Add a new item
    public void Add(Item item)
    {
        items.Add(item);
    }

    // Remove an item
    public void Remove(Item item)
    {
        items.Remove(item);
    }
}
