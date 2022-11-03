using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PercentageRandomness : MonoBehaviour
{
    public List<RandomItem> items;

    void Awake()
    {
        if (items == null)
        {
            items = new List<RandomItem>();
        }
    }

    public Sound PickSound()
    {
        if (items.Count == 0)
        {
            Debug.LogWarning("items is empty but PickSound() is still being called");
            return null;
        }

        //get number of valid points
        int numberOfValidPoints = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].count > 0)
            {
                numberOfValidPoints += items[i].count;
            }
        }

        //check for reset
        if (numberOfValidPoints < 1)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].count = items[i].defaultCount;
                numberOfValidPoints += items[i].count;
            }
        }

        //random picker
        int randomIndex = Random.Range(0, numberOfValidPoints);
        int validPoints = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].count > 0)
            {
                validPoints += items[i].count;
            }

            if (validPoints > randomIndex)
            {
                items[i].count--;
                return items[i].sound;
            }
        }

        Debug.LogError("PickSound() has failed");
        return null;
    }
}

[System.Serializable]
public class RandomItem
{
    public int count;
    public int defaultCount;
    public Sound sound;
}
