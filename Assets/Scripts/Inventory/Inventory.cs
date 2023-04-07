using System.Collections.Generic; //For lists
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new();

    private void Start()
    {
        if (gameObject.CompareTag("Chest")) //If this inventory is on a chest
        {
            foreach (Item item in items)
            {
                item.withinChest = true;
            }
        }
    }

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
