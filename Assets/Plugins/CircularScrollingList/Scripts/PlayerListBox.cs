using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirFishLab.ScrollingList;
using TMPro;

public class PlayerListBox : ListBox
{
    [SerializeField]
    private TextMeshProUGUI _contentText;

    // This function is invoked by the `CircularScrollingList` for updating the list content.
    // The type of the content will be converted to `object` in the `IntListBank` (Defined later)
    // So it should be converted back to its own type for being used.
    // The original type of the content is `int`.
    protected override void UpdateDisplayContent(object content)
    {
        _contentText.text = content.ToString();
    }
}
