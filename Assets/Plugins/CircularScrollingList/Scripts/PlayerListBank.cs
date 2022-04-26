using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirFishLab.ScrollingList;

[System.Serializable]
public class PlayerListBank : BaseListBank
{
    public List<string> attackNames;

    public void ChangeContents()
    {
        attackNames.Add("Wallop");
        GameObject.Find("Attack List").GetComponent<CircularScrollingList>().Refresh();
    }

    public override object GetListContent(int index)
    {
        return attackNames[index].ToString();
    }

    public override int GetListLength()
    {
        return attackNames.Count;
    }

    public void ListInit()
    {
        GetComponent<CircularScrollingList>().Initialize();
    }
}
