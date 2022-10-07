using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    void Awake()
    {
        instance = this;
    }

    public List<Item> items = new List<Item>();

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
